using System.Text.Json.Serialization;

namespace Tilde.MT.TranslationAPIService.Models.External.LanguageDirectionService.LanguageDirections
{
    public record LanguageDirection
    {
        /// <summary>
        /// The language of the source text. Two-byte language code according to ISO 639-1.
        /// </summary>
        /// <example>en</example>
        [JsonPropertyName("srcLang")]
        public string SourceLanguage { get; init; }

        /// <summary>
        /// The language to translate text to. Two-byte language code according to ISO 639-1.
        /// </summary>
        /// <example>et</example>
        [JsonPropertyName("trgLang")]
        public string TargetLanguage { get; init; }

        /// <summary>
        /// Text domain of the translation system to use for producing the translation.
        /// </summary>
        /// <example>general</example>
        [JsonPropertyName("domain")]
        public string Domain { get; init; }
    }
}
