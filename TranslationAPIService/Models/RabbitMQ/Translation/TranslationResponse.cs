using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation
{
    public class TranslationResponse: RabbitMQGenericMessage
    {
        /// <summary>
        /// Translation result
        /// </summary>
        [JsonProperty("translation")]
        public string[] Translations { get; set; }
    }
}
