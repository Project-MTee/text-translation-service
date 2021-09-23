using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation
{
    public class TranslationRequest
    {
        [JsonPropertyName("text")]
        public List<string> Text { get; set; }
        [JsonPropertyName("src")]
        public string SourceLanguage { get; set; }
        [JsonPropertyName("tgt")]
        public string TargetLanguage { get; set; }
        [JsonPropertyName("domain")]
        public string Domain { get; set; }
    }
}
