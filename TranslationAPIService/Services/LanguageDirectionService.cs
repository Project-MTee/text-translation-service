using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tilde.MT.TranslationAPIService.Exceptions.LanguageDirection;
using Tilde.MT.TranslationAPIService.Models;
using Tilde.MT.TranslationAPIService.Models.Configuration;
using Tilde.MT.TranslationAPIService.Models.LanguageDirectionService.LanguageDirections;

namespace Tilde.MT.TranslationAPIService.Services
{
    public class LanguageDirectionService: ILanguageDirectionService
    {
        private readonly ILogger _logger;
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ConfigurationServices _serviceConfiguration;

        private readonly SemaphoreSlim semaphore = new(1, 1);

        public LanguageDirectionService(
            ILogger<LanguageDirectionService> logger,
            IMemoryCache memoryCache,
            IHttpClientFactory clientFactory,
            IOptions<ConfigurationServices> serviceConfiguration
        )
        {
            _logger = logger;
            _cache = memoryCache;
            _clientFactory = clientFactory;
            _serviceConfiguration = serviceConfiguration.Value;
        }

        /// <summary>
        /// Fetch language directions from external language direction service or get them from cache
        /// </summary>
        /// <returns></returns>
        /// <exception cref="LanguageDirectionReadException">Failed to load language directions</exception>
        private async Task<IEnumerable<LanguageDirection>> Read()
        {
            try
            {
                await semaphore.WaitAsync();

                var languageDirections = await _cache.GetOrCreateAsync(MemoryCacheKeys.LanguageDirections, async (cacheEntry) =>
                {
                    var client = _clientFactory.CreateClient();

                    var response = await client.GetAsync($"{_serviceConfiguration.TranslationSystem.Url.TrimEnd(new char[] { '/', '\\', ' ' })}/LanguageDirection");

                    response.EnsureSuccessStatusCode();

                    var jsonString = await response.Content.ReadAsStringAsync();

                    var languageDirections = JsonSerializer.Deserialize<LanguageDirectionsResponse>(jsonString);

                    cacheEntry.SetAbsoluteExpiration(_serviceConfiguration.TranslationSystem.CacheTTL);

                    return languageDirections.LanguageDirections;
                });

                return languageDirections;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update language directions");
                throw new LanguageDirectionReadException();
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Check if language direction is available
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="sourceLanguage"></param>
        /// <param name="targetLanguage"></param>
        /// <returns></returns>
        /// <exception cref="LanguageDirectionReadException">Failed to load language directions</exception>
        /// <exception cref="LanguageDirectionNotFoundException">Language direction not found</exception>
        public async Task Validate(string domain, string sourceLanguage, string targetLanguage)
        {
            var languageDirections = await Read();

            // check if language direction exists.
            var valid = languageDirections.Any(item =>
            {
                var languageMatches = item.SourceLanguage == sourceLanguage &&
                    item.TargetLanguage == targetLanguage;

                var domainMatches = string.IsNullOrEmpty(domain) || item.Domain == domain;

                return domainMatches && languageMatches;
            });

            if (!valid)
            {
                throw new LanguageDirectionNotFoundException(domain, sourceLanguage, targetLanguage);
            }
        }
    }
}
