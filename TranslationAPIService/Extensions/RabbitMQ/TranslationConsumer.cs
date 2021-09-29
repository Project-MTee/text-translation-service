using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Extensions.RabbitMQ
{
    public class TranslationConsumer: IConsumer<Models.RabbitMQ.Translation.TranslationResponse>
    {
        readonly ILogger<TranslationConsumer> _logger;

        public TranslationConsumer(ILogger<TranslationConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<Models.RabbitMQ.Translation.TranslationResponse> context)
        {
            _logger.LogInformation($"Content Received: {context.Message.bar}");
            
            return Task.CompletedTask;
        }
    }


    public class TranslationConsumerDefinition: ConsumerDefinition<TranslationConsumer>
    {
        public TranslationConsumerDefinition()
        {
            EndpointName = $"translation.response";
        }

        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<TranslationConsumer> consumerConfigurator
        )
        {
            endpointConfigurator.ConfigureConsumeTopology = false;

            if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
            {
                rmq.Bind<Models.RabbitMQ.Translation.TranslationResponse>(x =>
                {
                    x.RoutingKey = "";
                    x.ExchangeType = ExchangeType.Fanout;
                });
            }
        }
    }
}
