using Newtonsoft.Json;

namespace Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation
{
    public record TranslationResponse : RabbitMQGenericResponse
    {
        /// <summary>
        /// Translation result
        /// </summary>
        [JsonProperty("translation")]
        public string[] Translations { get; init; }
    }
}