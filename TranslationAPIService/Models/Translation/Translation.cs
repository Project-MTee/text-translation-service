﻿using System;
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
        /// <example>general</example>
        [JsonPropertyName("domain")]
        public string Domain { get; set; }

        /// <summary>
        /// Translation results
        /// </summary>
        /// <example>[{"translation":"Translated segment"}]</example>
        [JsonPropertyName("translations")]
        public List<TranslationItem> Translations { get; set; }

        [JsonPropertyName("error")]
        public Error Error { get; set; }
    }
}
