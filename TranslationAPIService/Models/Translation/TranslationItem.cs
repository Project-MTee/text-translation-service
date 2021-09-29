using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models.Translation
{
    /// <summary>
    /// Translated information for one translation
    /// </summary>
    public class TranslationItem
    {
        /// <summary>
        /// Translated text
        /// </summary>
        [JsonPropertyName("translation")]
        public string Translation { get; set; }
    }
}
