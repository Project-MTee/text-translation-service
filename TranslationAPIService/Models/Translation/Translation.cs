using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models.Translation
{
    public class Translation: APIResponse
    {
        /// <summary>
        /// The text domain of the translation system used to produce the translation. This property contain automatically detected domain if not specified within the request.
        /// </summary>
        [JsonPropertyName("domain")]
        public string Domain { get; set; }
        /// <summary>
        /// Translation results
        /// </summary>
        [JsonPropertyName("translations")]
        public List<TranslationItem> Translations { get; set; }
    }
}
