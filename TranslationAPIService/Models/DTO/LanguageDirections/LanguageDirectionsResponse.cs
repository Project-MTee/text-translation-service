using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tilde.MT.TranslationAPIService.Models.DTO.LanguageDirections
{
    public class LanguageDirectionsResponse
    {
        [JsonPropertyName("languageDirections")]
        public IEnumerable<LanguageDirection> LanguageDirections { get; set; }
    }
}
