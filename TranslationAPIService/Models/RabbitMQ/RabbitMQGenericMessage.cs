using Newtonsoft.Json;

namespace Tilde.MT.TranslationAPIService.Models.RabbitMQ
{
    public class RabbitMQGenericMessage
    {
        /// <summary>
        /// HTTP status code (integer), 200 by default
        /// </summary>
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
        /// <summary>
        /// human-readable error message or "OK"
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
