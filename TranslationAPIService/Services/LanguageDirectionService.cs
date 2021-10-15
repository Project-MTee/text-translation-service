﻿using Microsoft.Extensions.Caching.Memory;
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

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        
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

        private async Task<List<LanguageDirection>> Read()
        {
            try
            {
                await semaphore.WaitAsync();

                if (_cache.Get<List<LanguageDirection>>(MemoryCacheKeys.LanguageDirections) == null)
                {
                    var client = _clientFactory.CreateClient();

                    var response = await client.GetAsync($"{_serviceConfiguration.TranslationSystem.Url.TrimEnd(new char[] { '/' , '\\', ' '})}/LanguageDirection");
                    
                    response.EnsureSuccessStatusCode();

                    var jsonString = await response.Content.ReadAsStringAsync();

                    var languageDirections = JsonSerializer.Deserialize<GetLanguageDirections>(jsonString);

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

            return _cache.Get<List<LanguageDirection>>(MemoryCacheKeys.LanguageDirections);
        }
    
        public async Task<bool> Validate(RequestTranslation request)
        {
            var languageDirections = await Read();

            if(languageDirections == null)
            {
                throw new LanguageDirectionsException("Failed to load language directions");
            }

            // check if language direction exists.
            var languageDirectionInSettings = languageDirections.Where(item =>
            {
                var languageMatches = item.SourceLanguage == request.SourceLanguage &&
                    item.TargetLanguage == request.TargetLanguage;

                var domainMatches = string.IsNullOrEmpty(request.Domain) || item.Domain == request.Domain;

                return domainMatches && languageMatches;
            });

            return languageDirectionInSettings.Any();
        }
    }
}
