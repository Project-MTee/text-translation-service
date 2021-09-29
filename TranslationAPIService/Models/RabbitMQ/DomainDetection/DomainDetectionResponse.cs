using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models.RabbitMQ.DomainDetection
{
    public class DomainDetectionResponse: RabbitMQGenericMessage
    {
        /// <summary>
        /// string of the detected domain name
        /// </summary>
        [JsonPropertyName("domain")]
        public string Domain { get; set; }
    }
}
