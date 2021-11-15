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
using Tilde.MT.TranslationAPIService.Exceptions;
using Tilde.MT.TranslationAPIService.Models;
using Tilde.MT.TranslationAPIService.Models.Configuration;
using Tilde.MT.TranslationAPIService.Models.DTO.LanguageDirections;
using Tilde.MT.TranslationAPIService.Models.DTO.Translation;

namespace Tilde.MT.TranslationAPIService.Services
{
    public class LanguageDirectionService
    {
        private readonly ILogger _logger;
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ConfigurationServices _serviceConfiguration;

        private readonly SemaphoreSlim semaphore = new(1, 1);

        private readonly TimeSpan expiration = TimeSpan.FromHours(1);
        
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

                    cacheEntry.SetAbsoluteExpiration(expiration);

                    return languageDirections.LanguageDirections;
                });

                return languageDirections;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update language directions");
                throw new LanguageDirectionsException("Failed to load language directions");
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Check if language direction is available
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="LanguageDirectionsException">Failed to load language directions</exception>
        public async Task<bool> Validate(TranslationRequest request)
        {
            var languageDirections = await Read();

            // check if language direction exists.
            var valid = languageDirections.Any(item =>
            {
                var languageMatches = item.SourceLanguage == request.SourceLanguage &&
                    item.TargetLanguage == request.TargetLanguage;

                var domainMatches = string.IsNullOrEmpty(request.Domain) || item.Domain == request.Domain;

                return domainMatches && languageMatches;
            });

            return valid;
        }
    }
}
