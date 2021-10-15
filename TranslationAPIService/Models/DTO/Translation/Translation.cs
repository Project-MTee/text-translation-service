using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tilde.MT.TranslationAPIService.Models.DTO.Translation
{
    public class Translation
    {
        /// <summary>
        /// The text domain of the translation system used to produce the translation. This property contain automatically detected domain if not specified within the request.
        /// </summary>
        /// <example>general</example>
        [JsonPropertyName("domain")]
        public string Domain { get; set; }

        /// <summary>
        /// Translation results
        /// </summary>
        /// <example>[{"translation":"Translated segment"}]</example>
        [JsonPropertyName("translations")]
        public List<TranslationItem> Translations { get; set; }
    }
}
