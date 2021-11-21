using System.Text.Json.Serialization;

namespace Tilde.MT.TranslationAPIService.Models.DTO.Translation
{
    /// <summary>
    /// Translated information for one translation
    /// </summary>
    public record TranslationItem
    {
        /// <summary>
        /// Translated text
        /// </summary>
        [JsonPropertyName("translation")]
        public string Translation { get; init; }
    }
}
