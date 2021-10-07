using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models.Configuration
{
    public class ConfigurationSettings
    {
        public TimeSpan TranslationTimeout { get; set; }
        public TimeSpan DomainDetectionTimeout { get; set; }
    }
}
