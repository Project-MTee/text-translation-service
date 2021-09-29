using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models.RabbitMQ
{
    public class RabbitMQGenericMessage
    {
        /// <summary>
        /// HTTP status code (integer), 200 by default
        /// </summary>
        [JsonPropertyName("status_code")]
        public int StatusCode { get; set; }
        /// <summary>
        /// human-readable error message or "OK"
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
