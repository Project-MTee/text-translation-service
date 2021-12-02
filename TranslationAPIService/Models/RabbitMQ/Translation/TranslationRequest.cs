using Newtonsoft.Json;
using System.Collections.Generic;

namespace Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation
{
    public record TranslationRequest
    {
        [JsonProperty("text")]
        public IEnumerable<string> Text { get; init; }
        [JsonProperty("src")]
        public string SourceLanguage { get; init; }
        [JsonProperty("tgt")]
        public string TargetLanguage { get; init; }
        [JsonProperty("domain")]
        public string Domain { get; init; }
        [JsonProperty("input_type")]
        public string InputType { get; init; }
    }
}
