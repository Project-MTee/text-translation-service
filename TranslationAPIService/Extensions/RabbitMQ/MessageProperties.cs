using MassTransit;
using MassTransit.RabbitMqTransport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Extensions.RabbitMQ
{
    public static class MessageProperties
    {
        public static void AddReplyToProperty(this SendContext context)
        {
            if (!context.TryGetPayload(out RabbitMqSendContext sendContext))
            {
                throw new ArgumentException("The RabbitMqSendContext was not available");
            }
            sendContext.BasicProperties.ReplyTo = context.ResponseAddress.ToString();
        }
    }
}
