using FluentAssertions;
using MemoryCache.Testing.Moq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tilde.MT.TranslationAPIService.Exceptions.LanguageDirection;
using Tilde.MT.TranslationAPIService.Models;
using Tilde.MT.TranslationAPIService.Models.Configuration;
using Tilde.MT.TranslationAPIService.Models.Configuration.Services;
using Tilde.MT.TranslationAPIService.Models.LanguageDirectionService.LanguageDirections;
using Xunit;

namespace TranslationAPIService.Tests.UnitTests.LanguageDirectionService
{
    public class LanguageDirectionServiceTests
    {
        private readonly IOptions<ConfigurationServices> options;
        private readonly HttpResponseMessage apiResponse;
        private readonly Mock<HttpMessageHandler> httpClientHandler;
        private readonly IHttpClientFactory httpClientFactory;

        public LanguageDirectionServiceTests()
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

            httpClientHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            httpClientHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(apiResponse)
                .Verifiable();

            var httpClient = new HttpClient(httpClientHandler.Object);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClientFactory.Setup(_ => _.CreateClient(string.Empty)).Returns(httpClient);

            httpClientFactory = mockHttpClientFactory.Object;
        }

        [Fact]
        public async Task CacheIsNotUsed_WhenCacheIsNotExpired()
        {
            // --- Arrange
            var domain = "finance";
            var sourceLanguage = "et";
            var targetLanguage = "en";

            var service = new Tilde.MT.TranslationAPIService.Services.LanguageDirectionService(
                Mock.Of<ILogger<Tilde.MT.TranslationAPIService.Services.LanguageDirectionService>>(),
                Create.MockedMemoryCache(),
                httpClientFactory,
                options
            );

            var tcs = new TaskCompletionSource();
            var ctsStore = new CancellationTokenSource();

            // --- Act

            // Store cache from api response
            await service.Validate(domain, sourceLanguage, targetLanguage);

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

            httpClientHandler.Protected().Verify(
               "SendAsync",
               Times.Exactly(1),
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Get
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task CacheIsNotUsed_WhenCacheIsExpired()
        {
            // --- Arrange
            
            var domain = "finance";
            var sourceLanguage = "et";
            var targetLanguage = "en";

            var memoryCache = Mock.Of<IMemoryCache>();
            var cachEntry = Mock.Of<ICacheEntry>();

            var mockMemoryCache = Mock.Get(memoryCache);
            mockMemoryCache
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cachEntry);

            var service = new Tilde.MT.TranslationAPIService.Services.LanguageDirectionService(
                Mock.Of<ILogger<Tilde.MT.TranslationAPIService.Services.LanguageDirectionService>>(),
                mockMemoryCache.Object,
                httpClientFactory,
                options
            );

            // --- Act

            // Store cache from api response
            await service.Validate(domain, sourceLanguage, targetLanguage);

            // Hack to simulate mocked memory cache TTL
            memoryCache.Remove(MemoryCacheKeys.LanguageDirections);

            await service.Validate(domain, sourceLanguage, targetLanguage);

            // --- Assert

            httpClientHandler.Protected().Verify(
               "SendAsync",
               Times.Exactly(2),
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Get
               ),
               ItExpr.IsAny<CancellationToken>()
            );
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
            
            var service = new Tilde.MT.TranslationAPIService.Services.LanguageDirectionService(
                Mock.Of<ILogger<Tilde.MT.TranslationAPIService.Services.LanguageDirectionService>>(),
                Create.MockedMemoryCache(),
                httpClientFactory,
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

                httpClientHandler.Protected().Verify(
                   "SendAsync",
                   Times.Exactly(1),
                   ItExpr.Is<HttpRequestMessage>(req =>
                      req.Method == HttpMethod.Get
                   ),
                   ItExpr.IsAny<CancellationToken>()
                );
            }
            else
            {
                exception.Should().BeOfType<LanguageDirectionNotFoundException>();
            }
        }
    }
}
