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

namespace TranslationAPIService.Tests.UnitTests.LanguageDirectionService
{
    public class LanguageDirectionServiceTest
    {
        private readonly IOptions<ConfigurationServices> options;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IMemoryCache memoryCache;
        private readonly HttpResponseMessage apiResponse;

        public LanguageDirectionServiceTest()
        {
            options = Options.Create(new ConfigurationServices()
            {
                TranslationSystem = new TranslationSystem()
                {
                    Url = "http://localhost/",
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

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Returns(() =>
                {
                    return Task.FromResult(apiResponse);
                })
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClientFactory.Setup(_ => _.CreateClient(string.Empty)).Returns(httpClient);

            httpClientFactory = mockHttpClientFactory.Object;


            var memoryCache = Mock.Of<IMemoryCache>();
            var cachEntry = Mock.Of<ICacheEntry>();

            var mockMemoryCache = Mock.Get(memoryCache);
            mockMemoryCache
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cachEntry);

            this.memoryCache = mockMemoryCache.Object;
        }

        [Fact]
        public async Task LanguageDirectionCacheIsStoredAndReset()
        {
            // Arrange

            var apiRequested = false;

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Returns(() =>
                {
                    apiRequested = true;
                    return Task.FromResult(apiResponse);
                })
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClientFactory.Setup(_ => _.CreateClient(string.Empty)).Returns(httpClient);

            var httpClientFactory = mockHttpClientFactory.Object;

            var service = new Tilde.MT.TranslationAPIService.Services.LanguageDirectionService(
                Mock.Of<ILogger<Tilde.MT.TranslationAPIService.Services.LanguageDirectionService>>(),
                memoryCache,
                httpClientFactory,
                options
            );

            // Act

            var tcs = new TaskCompletionSource<bool>();
            var ctsStore = new CancellationTokenSource();
            // Fill cache
            await service.Validate("", "en", "et");

            var task = service.Validate("", "en", "et");

            
            ctsStore.Token.Register(() =>
            {
                // Check if cache is stored
                tcs.SetResult(apiRequested);
            });
            ctsStore.CancelAfter(options.Value.TranslationSystem.CacheTTL / 2);

            bool apiRequestUsedBeforeTTL = await tcs.Task;
            await task;

            await Task.Delay(options.Value.TranslationSystem.CacheTTL + TimeSpan.FromSeconds(1));

            await service.Validate("", "en", "et");

            // Assert

            // Check if cache is used insteal of always relying on external api
            Assert.False(apiRequestUsedBeforeTTL);

            // Check if external api is used after ttl
            Assert.True(apiRequested);
        }

        /// <summary>
        /// Check if language direction is validated correctly
        /// </summary>
        /// <param name="sourceLanguage"></param>
        /// <param name="targetLanguage"></param>
        /// <param name="domain"></param>
        /// <param name="valid"></param>
        /// <returns></returns>
        [Theory]
        // with domain
        [InlineData("en", "et", "general", true)]
        [InlineData("en", "---", "general", false)]
        [InlineData("---", "et", "general", false)]
        // no domain
        [InlineData("en", "et", "", true)]
        [InlineData("en", "---", "", false)]
        [InlineData("---", "et", "", false)]
        public async Task LanguageDirectionValidationIsValid(string sourceLanguage, string targetLanguage, string domain, bool valid)
        {
            // Arrange
            var service = new Tilde.MT.TranslationAPIService.Services.LanguageDirectionService(
                Mock.Of<ILogger<Tilde.MT.TranslationAPIService.Services.LanguageDirectionService>>(),
                memoryCache,
                httpClientFactory,
                options
            );

            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                await service.Validate(domain, sourceLanguage, targetLanguage);
            });

            // Assert
            if (valid)
            {
                Assert.Null(exception);
            }
            else
            {
                Assert.IsType<LanguageDirectionNotFoundException>(exception);
            }
        }
    }
}
