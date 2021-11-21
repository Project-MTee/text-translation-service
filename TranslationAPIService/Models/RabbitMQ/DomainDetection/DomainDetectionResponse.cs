using Newtonsoft.Json;

namespace Tilde.MT.TranslationAPIService.Models.RabbitMQ.DomainDetection
{
    public record DomainDetectionResponse : RabbitMQGenericResponse
    {
        /// <summary>
        /// detected domain
        /// </summary>
        [JsonProperty("domain")]
        public string Domain { get; init; }
    }
}
