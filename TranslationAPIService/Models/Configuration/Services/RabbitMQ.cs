using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models.Configuration.Services
{
    public class RabbitMQ
    {
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string TranslationExchangeName { get; set; }
        public string LanguageDetectionExchangeName { get; set; }
    }
}
