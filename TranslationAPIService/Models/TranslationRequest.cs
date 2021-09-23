﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models
{
    public class TranslationRequest
    {
        /// <summary>
        /// The language of the source text. Two-byte languge code accordding to ISO 639-1.
        /// </summary>
        [Required]
        [MaxLength(2)]
        [JsonPropertyName("srcLang")]
        public string SourceLanguage { get; set; }
        /// <summary>
        /// The language to translate text to. Two-byte languge code according to ISO 639-1.
        /// </summary>
        [Required]
        [MaxLength(2)]
        [JsonPropertyName("trgLang")]
        public string TargetLanguage { get; set; }
        /// <summary>
        /// (Optional) Text domain of the translation system to use for producing the translation. The domain is going to be detected automatically if not specified.
        /// </summary>
        [MaxLength(200)]
        [JsonPropertyName("domain")]
        public string Domain { get; set; }
        /// <summary>
        /// Array of text segments to translate
        /// </summary>
        [JsonPropertyName("text")] 
        public List<string> Text { get; set; }
    }
}
