using Newtonsoft.Json;

namespace Tilde.MT.TranslationAPIService.Models.RabbitMQ
{
    public record RabbitMQGenericResponse
    {
        /// <summary>
        /// HTTP status code (integer), 200 by default
        /// </summary>
        [JsonProperty("status_code")]
        public int StatusCode { get; init; }
        /// <summary>
        /// human-readable error message or "OK"
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; init; }
    }
}
