using System;

namespace Tilde.MT.TranslationAPIService.Models.Configuration.Services
{
    public record TranslationSystem
    {
        public string Url { get; init; }
        public TimeSpan CacheTTL { get; init; } = TimeSpan.FromHours(1);
    }
}
