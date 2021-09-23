using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models.RabbitMQ.DomainDetection
{
    public class DomainDetectionRequest
    {
        /// <summary>
        /// input text. Can be a string (may contain multiple sentences or paragraphs) or a list of strings (may not contain more than one segment)
        /// </summary>
        [JsonPropertyName("text")]
        public List<string> Text { get; set; }
        /// <summary>
        /// 2-letter source language code 
        /// </summary>
        [JsonPropertyName("src")]
        public string SourceLanguage { get; set; }
    }
}
