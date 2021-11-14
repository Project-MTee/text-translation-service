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
        /// <summary>
        /// Request size limit in bytes.
        /// For example: 20480 -> 20KB
        /// </summary>
        public long RequestSizeLimit { get; set; }
    }
}
