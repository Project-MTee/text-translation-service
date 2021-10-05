using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models.LanguageDetection
{
    public class GetLanguageDirections: APIResponse
    {
        [JsonPropertyName("languageDirections")]
        public IEnumerable<LanguageDirection> LanguageDirections { get; set; }

        [JsonPropertyName("error")]
        public Error Error { get; set; }
    }
}
