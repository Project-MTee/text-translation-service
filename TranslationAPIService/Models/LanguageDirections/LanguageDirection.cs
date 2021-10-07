using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models.LanguageDirections
{
    public class LanguageDirection
    {
        /// <summary>
        /// The language of the source text. Two-byte languge code accordding to ISO 639-1.
        /// </summary>
        /// <example>en</example>
        [JsonPropertyName("srcLang")]
        public string SourceLanguage { get; set; }

        /// <summary>
        /// The language to translate text to. Two-byte languge code according to ISO 639-1.
        /// </summary>
        /// <example>et</example>
        [JsonPropertyName("trgLang")]
        public string TargetLanguage { get; set; }

        /// <summary>
        /// Text domain of the translation system to use for producing the translation.
        /// </summary>
        /// <example>general</example>
        [JsonPropertyName("domain")]
        public string Domain { get; set; }
    }
}
