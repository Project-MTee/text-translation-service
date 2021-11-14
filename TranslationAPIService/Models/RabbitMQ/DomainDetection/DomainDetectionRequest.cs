using Newtonsoft.Json;
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
        /// Text to detect domain for
        /// </summary>
        [JsonProperty("text")]
        public List<string> Text { get; set; }
        /// <summary>
        /// 2-letter source language code 
        /// </summary>
        [JsonProperty("src")]
        public string SourceLanguage { get; set; }
    }
}
