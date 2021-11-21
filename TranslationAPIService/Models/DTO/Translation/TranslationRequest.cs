using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tilde.MT.TranslationAPIService.Models.DTO.Translation
{
    public record TranslationRequest
    {
        /// <summary>
        /// The language of the source text. Two-byte language code according to ISO 639-1.
        /// </summary>
        /// <example>en</example>
        [Required]
        [MaxLength(2)]
        [JsonPropertyName("srcLang")]
        public string SourceLanguage { get; init; }

        /// <summary>
        /// The language to translate text to. Two-byte language code according to ISO 639-1.
        /// </summary>
        /// <example>et</example>
        [Required]
        [MaxLength(2)]
        [JsonPropertyName("trgLang")]
        public string TargetLanguage { get; init; }

        /// <summary>
        /// (Optional) Text domain of the translation system to use for producing the translation.The domain is going to be detected automatically if not specified.
        /// </summary>
        /// <example>general</example>
        [MaxLength(200)]
        [JsonPropertyName("domain")]
        public string Domain { get; init; }

        /// <summary>
        /// Array of text segments to translate
        /// </summary>
        /// <example>["Segment to translate"]</example>
        [JsonPropertyName("text")]
        public List<string> Text { get; init; }

        /// <summary>
        /// Input text type. Default: plaintext
        /// </summary>
        [JsonPropertyName("textType")]
        public Enums.TranslationType InputType { get; init; } = Enums.TranslationType.plain;
    }
}
