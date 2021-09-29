using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tilde.MT.TranslationAPIService.Models.Configuration;

namespace Tilde.MT.TranslationAPIService.Extensions.RabbitMQ
{
    public class RequestClient<Treq, Tres> : RawRequestClient<Treq, Tres>
    {
        public RequestClient(IOptions<ConfigurationServices> serviceOptions) :base(
            serviceOptions.Value.RabbitMQ.Host, 
            serviceOptions.Value.RabbitMQ.UserName,
            serviceOptions.Value.RabbitMQ.Password,
            serviceOptions.Value.RabbitMQ.Port
        )
        {

        }
    }

    public class RawRequestClient<Treq, Tres>
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly EventingBasicConsumer consumer;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<Tres>> callbackMapper = new ConcurrentDictionary<string, TaskCompletionSource<Tres>>();

        public RawRequestClient(string host, string userName, string password, int port = 5672)
        {
            var factory = new ConnectionFactory() {
                HostName = host,
                UserName = userName,
                Password = password,
                Port = port,
                ClientProvidedName = $"{Assembly.GetEntryAssembly().GetName().Name} ({typeof(Treq).Name})"
            };
            factory.AutomaticRecoveryEnabled = true;
            factory.TopologyRecoveryEnabled = true;

            connection = factory.CreateConnection();
            connection.ConnectionUnblocked += Connection_ConnectionUnblocked;
            channel = connection.CreateModel();
            // declare a server-named queue
            channel.QueueDeclare(queue: "");
            consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, message) =>
            {
                if (!callbackMapper.TryRemove(message.BasicProperties.CorrelationId, out TaskCompletionSource<Tres> tcs))
                {
                    return;
                }

                try
                {
                    var body = message.Body.ToArray();
                    var responseJson = Encoding.UTF8.GetString(body);
                    var response = JsonSerializer.Deserialize<Tres>(responseJson);
                    tcs.TrySetResult(response);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            };

            channel.BasicConsume(
                consumer: consumer,
                queue: "",
                autoAck: true
            );
        }

        private void Connection_ConnectionUnblocked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public Task<Tres> GetResponse(Treq message, string routingKey, string exchange, TimeSpan timeout)
        {
            var cts = new CancellationTokenSource();

            IBasicProperties props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();

            props.CorrelationId = correlationId;
            props.ReplyTo = "";

            var jsonMessage = JsonSerializer.Serialize(message);
            var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);
            
            var tcs = new TaskCompletionSource<Tres>();

            callbackMapper.TryAdd(correlationId, tcs);
            cts.CancelAfter(timeout);

            channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: props,
                body: messageBytes
            );

            cts.Token.Register(() => {
                callbackMapper.TryRemove(correlationId, out var tmp);
                tcs.TrySetException(new TimeoutException($"Request timed out: {correlationId}"));
            });

            return tcs.Task;
        }

        ~RawRequestClient()
        {
            connection.Close();
        }
    }
}
