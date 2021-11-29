using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tilde.MT.TranslationAPIService.Models.LanguageDirectionService.LanguageDirections
{
    public record LanguageDirectionsResponse
    {
        [JsonPropertyName("languageDirections")]
        public IEnumerable<LanguageDirection> LanguageDirections { get; init; }
    }
}
