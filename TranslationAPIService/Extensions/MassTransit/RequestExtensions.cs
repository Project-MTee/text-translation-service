using MassTransit;
using MassTransit.RabbitMqTransport;
using MassTransit.Registration;
using System;

namespace Tilde.MT.TranslationAPIService.Extensions.MassTransit
{
    public static class RequestExtensions
    {
        /// <summary>
        /// Add custom request headers to RabbitMQ message for integration with raw RabbitMQ consumers (non MassTransit, for example python)
        /// </summary>
        /// <typeparam name="Tresponse">RabbitMQ response message type</typeparam>
        /// <param name="context">Mass Transit send context</param>
        /// <exception cref="ArgumentException">RabbitMqSendContext was not available</exception>
        public static void AddRequestHeaders<Tresponse>(this SendContext context)
        {
            if (!context.TryGetPayload(out RabbitMqSendContext sendContext))
            {
                throw new ArgumentException("RabbitMqSendContext was not available");
            }
            sendContext.BasicProperties.ReplyTo = context.ResponseAddress.GetLastPart();
            sendContext.Headers.Set("RequestId", sendContext.RequestId?.ToString());
            sendContext.Headers.Set("ReturnMessageType", $"urn:message:{typeof(Tresponse).Namespace}:{typeof(Tresponse).Name}");
        }
    }
}
