using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models.LanguageDetection
{
    public class LanguageDirection
    {
        [JsonPropertyName("srcLang")]
        public string SourceLanguage { get; set; }
        [JsonPropertyName("trgLang")]
        public string TargetLanguage { get; set; }
        [JsonPropertyName("domain")]
        public string Domain { get; set; }
    }
}
