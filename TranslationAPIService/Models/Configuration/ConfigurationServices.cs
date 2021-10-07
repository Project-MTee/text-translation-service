using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models.Configuration
{
    public class ConfigurationServices
    {
        public Services.RabbitMQ RabbitMQ { get; set; }
        public Services.TranslationSystem TranslationSystem { get; set; }
    }
}
