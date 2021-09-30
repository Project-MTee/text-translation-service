using Newtonsoft.Json;
using System.Collections.Generic;

namespace Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation
{
    public class TranslationRequest
    {
        [JsonProperty("text")]
        public List<string> Text { get; set; }
        [JsonProperty("src")]
        public string SourceLanguage { get; set; }
        [JsonProperty("tgt")]
        public string TargetLanguage { get; set; }
        [JsonProperty("domain")]
        public string Domain { get; set; }
        [JsonProperty("input_type")]
        public string InputType { get; set; }
    }
}
