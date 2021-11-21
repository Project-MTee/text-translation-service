using Newtonsoft.Json;
using System.Collections.Generic;

namespace Tilde.MT.TranslationAPIService.Models.RabbitMQ.DomainDetection
{
    public record DomainDetectionRequest
    {
        /// <summary>
        /// Text to detect domain for
        /// </summary>
        [JsonProperty("text")]
        public List<string> Text { get; init; }
        /// <summary>
        /// 2-letter source language code 
        /// </summary>
        [JsonProperty("src")]
        public string SourceLanguage { get; init; }
    }
}
