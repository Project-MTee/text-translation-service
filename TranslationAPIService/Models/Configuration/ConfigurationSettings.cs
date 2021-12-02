using System;

namespace Tilde.MT.TranslationAPIService.Models.Configuration
{
    public record ConfigurationSettings
    {
        public TimeSpan TranslationTimeout { get; init; }
        public TimeSpan DomainDetectionTimeout { get; init; }
        /// <summary>
        /// Request size limit in bytes.
        /// For example: 20480 -> 20KB
        /// </summary>
        public long RequestSizeLimit { get; init; }
        public int TranslationRequestSegmentCount { get; init; } = 3;
    }
}
