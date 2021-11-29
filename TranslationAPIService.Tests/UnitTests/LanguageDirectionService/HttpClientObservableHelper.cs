using Moq;
using Moq.Protected;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TranslationAPIService.Tests.UnitTests.LanguageDirectionService
{
    public class HttpClientObservableHelper
    {
        public int ApiRequests { get; private set; }
        public readonly IHttpClientFactory httpClientFactory;

        public HttpClientObservableHelper(HttpResponseMessage apiResponse)
        {
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
                    ApiRequests++;
                    return Task.FromResult(apiResponse);
                })
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClientFactory.Setup(_ => _.CreateClient(string.Empty)).Returns(httpClient);

            httpClientFactory = mockHttpClientFactory.Object;
        }
    }
}
