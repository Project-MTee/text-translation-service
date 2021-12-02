using Newtonsoft.Json;
using System.Collections.Generic;

namespace Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation
{
    public record TranslationResponse : RabbitMQGenericResponse
    {
        /// <summary>
        /// Translation result
        /// </summary>
        [JsonProperty("translation")]
        public IEnumerable<string> Translations { get; init; }
    }
}