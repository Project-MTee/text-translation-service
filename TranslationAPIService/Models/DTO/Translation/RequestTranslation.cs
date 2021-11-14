using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models.DTO.Translation
{
    public class RequestTranslation
    {
        /// <summary>
        /// The language of the source text. Two-byte languge code according to ISO 639-1.
        /// </summary>
        /// <example>en</example>
        [Required]
        [MaxLength(2)]
        [JsonPropertyName("srcLang")]
        public string SourceLanguage { get; set; }

        /// <summary>
        /// The language to translate text to. Two-byte languge code according to ISO 639-1.
        /// </summary>
        /// <example>et</example>
        [Required]
        [MaxLength(2)]
        [JsonPropertyName("trgLang")]
        public string TargetLanguage { get; set; }

        /// <summary>
        /// (Optional) Text domain of the translation system to use for producing the translation.The domain is going to be detected automatically if not specified.
        /// </summary>
        /// <example>general</example>
        [MaxLength(200)]
        [JsonPropertyName("domain")]
        public string Domain { get; set; }

        /// <summary>
        /// Array of text segments to translate
        /// </summary>
        /// <example>["Segment to translate"]</example>
        [JsonPropertyName("text")]
        public List<string> Text { get; set; }

        /// <summary>
        /// Input text type. Default: plaintext
        /// </summary>
        [JsonPropertyName("textType")]
        public Enums.TranslationType InputType { get; set; } = Enums.TranslationType.plain;
    }
}
