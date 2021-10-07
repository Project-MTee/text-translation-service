using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tilde.MT.TranslationAPIService.Models;
using Tilde.MT.TranslationAPIService.Models.Configuration;

namespace Tilde.MT.TranslationAPIService.Services
{
    public class LanguageDirectionService
    {
        private readonly ILogger _logger;
        private IMemoryCache _cache;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ConfigurationServices _serviceConfiguration;

        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        
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

        public async Task<List<Models.LanguageDirections.LanguageDirection>> Read()
        {
            try
            {
                await semaphore.WaitAsync();

                if (_cache.Get<List<Models.LanguageDirections.LanguageDirection>>(MemoryCacheKeys.LanguageDirections) == null)
                {
                    var client = _clientFactory.CreateClient();

                    var response = await client.GetAsync($"{_serviceConfiguration.TranslationSystem.Url.TrimEnd(new char[] { '/' , '\\', ' '})}/LanguageDirection");
                    
                    response.EnsureSuccessStatusCode();

                    var jsonString = await response.Content.ReadAsStringAsync();

                    var languageDirections = JsonSerializer.Deserialize<Models.LanguageDirections.GetLanguageDirections>(jsonString);

                    _cache.Set(MemoryCacheKeys.LanguageDirections, languageDirections.LanguageDirections, TimeSpan.FromHours(1));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update language directions");
            }
            finally
            {
                semaphore.Release();
            }

            return _cache.Get<List<Models.LanguageDirections.LanguageDirection>>(MemoryCacheKeys.LanguageDirections);
        }
    }
}
