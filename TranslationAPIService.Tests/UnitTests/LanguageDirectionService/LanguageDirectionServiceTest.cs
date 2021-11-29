using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tilde.MT.TranslationAPIService.Exceptions.LanguageDirection;
using Tilde.MT.TranslationAPIService.Models.Configuration;
using Tilde.MT.TranslationAPIService.Models.Configuration.Services;
using Tilde.MT.TranslationAPIService.Models.DTO.LanguageDirections;
using Xunit;
using FluentAssertions;
using MemoryCache.Testing.Moq;
using Tilde.MT.TranslationAPIService.Models;

namespace TranslationAPIService.Tests.UnitTests.LanguageDirectionService
{
    public class LanguageDirectionServiceTest
    {
        private readonly IOptions<ConfigurationServices> options;
        private readonly HttpResponseMessage apiResponse;

        public LanguageDirectionServiceTest()
        {
            options = Options.Create(new ConfigurationServices()
            {
                TranslationSystem = new TranslationSystem()
                {
                    Url = "http://foo.bar/",
                    CacheTTL = TimeSpan.FromSeconds(6)
                }
            });

            var languageDirections = new List<LanguageDirection>()
            {
                new LanguageDirection()
                {
                    Domain = "general",
                    SourceLanguage = "en",
                    TargetLanguage = "et"
                },
                new LanguageDirection()
                {
                    Domain = "finance",
                    SourceLanguage = "et",
                    TargetLanguage = "en"
                }
            };

            apiResponse = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    JsonSerializer.Serialize(
                        new LanguageDirectionsResponse()
                        {
                            LanguageDirections = languageDirections
                        }
                    )
                )
            };
        }

        [Fact]
        public async Task CacheIsNotUsed_WhenCacheIsNotExpired()
        {
            // --- Arrange
            var domain = "finance";
            var sourceLanguage = "et";
            var targetLanguage = "en";

            int apiRequestsOfset;
            var httpFactoryHelper = new HttpClientObservableHelper(apiResponse);

            var service = new Tilde.MT.TranslationAPIService.Services.LanguageDirectionService(
                Mock.Of<ILogger<Tilde.MT.TranslationAPIService.Services.LanguageDirectionService>>(),
                Create.MockedMemoryCache(),
                httpFactoryHelper.httpClientFactory,
                options
            );

            var tcs = new TaskCompletionSource();
            var ctsStore = new CancellationTokenSource();

            // --- Act

            // Store cache from api response
            await service.Validate(domain, sourceLanguage, targetLanguage);
            apiRequestsOfset = httpFactoryHelper.ApiRequests;

            var task = service.Validate(domain, sourceLanguage, targetLanguage);
            
            ctsStore.Token.Register(() =>
            {
                // Check if cache is stored
                tcs.SetResult();
            });
            ctsStore.CancelAfter(options.Value.TranslationSystem.CacheTTL / 2);

            await tcs.Task;
            await task;

            // --- Assert

            apiRequestsOfset.Should().Be(1);
            apiRequestsOfset.Should().Be(httpFactoryHelper.ApiRequests);
        }

        [Fact]
        public async Task CacheIsNotUsed_WhenCacheIsExpired()
        {
            // --- Arrange

            var domain = "finance";
            var sourceLanguage = "et";
            var targetLanguage = "en";

            int apiRequestsOfset;
            var httpFactoryHelper = new HttpClientObservableHelper(apiResponse);

            var memoryCache = Mock.Of<IMemoryCache>();
            var cachEntry = Mock.Of<ICacheEntry>();

            var mockMemoryCache = Mock.Get(memoryCache);
            mockMemoryCache
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cachEntry);

            var service = new Tilde.MT.TranslationAPIService.Services.LanguageDirectionService(
                Mock.Of<ILogger<Tilde.MT.TranslationAPIService.Services.LanguageDirectionService>>(),
                mockMemoryCache.Object,
                httpFactoryHelper.httpClientFactory,
                options
            );

            // --- Act

            // Store cache from api response
            await service.Validate(domain, sourceLanguage, targetLanguage);

            // Hack to simulate mocked memory cache TTL
            memoryCache.Remove(MemoryCacheKeys.LanguageDirections);

            apiRequestsOfset = httpFactoryHelper.ApiRequests;
            
            await service.Validate(domain, sourceLanguage, targetLanguage);

            // --- Assert

            apiRequestsOfset.Should().Be(1);
            httpFactoryHelper.ApiRequests.Should().Be(2);
        }

        [Theory]
        // with domain
        [InlineData("en", "et", "general", true)]
        [InlineData("en", "---", "general", false)]
        [InlineData("---", "et", "general", false)]
        // no domain
        [InlineData("en", "et", "", true)]
        [InlineData("en", "---", "", false)]
        [InlineData("---", "et", "", false)]
        public async Task ValidationProcessCorrect(string sourceLanguage, string targetLanguage, string domain, bool valid)
        {
            // --- Arrange

            var httpFactoryHelper = new HttpClientObservableHelper(apiResponse);

            var service = new Tilde.MT.TranslationAPIService.Services.LanguageDirectionService(
                Mock.Of<ILogger<Tilde.MT.TranslationAPIService.Services.LanguageDirectionService>>(),
                Create.MockedMemoryCache(),
                httpFactoryHelper.httpClientFactory,
                options
            );

            // --- Act

            var exception = await Record.ExceptionAsync(async () =>
            {
                await service.Validate(domain, sourceLanguage, targetLanguage);
            });

            // --- Assert
            if (valid)
            {
                exception.Should().BeNull();
            }
            else
            {
                exception.Should().BeOfType<LanguageDirectionNotFoundException>();
            }
        }
    }
}
