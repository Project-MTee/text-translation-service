using AutoMapper;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tilde.MT.TranslationAPIService.Exceptions.Translation;
using Tilde.MT.TranslationAPIService.Models.Configuration;
using Tilde.MT.TranslationAPIService.Models.DTO.Translation;
using Tilde.MT.TranslationAPIService.Models.RabbitMQ.DomainDetection;
using Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation;
using Xunit;

namespace TranslationAPIService.Tests.UnitTests.TranslationService
{
    public class TranslationServiceTests
    {
        private readonly TranslationServiceRequest translationRequest;
        private readonly IMapper mapper;

        public TranslationServiceTests()
        {
            translationRequest = new TranslationServiceRequest()
            {
                Domain = "finance",
                InputType = Tilde.MT.TranslationAPIService.Enums.TranslationType.plain,
                SourceLanguage = "en",
                TargetLanguage = "fr",
                Text = new List<string>()
                {
                    "text message",
                    "other text message"
                }
            };

            var mapper = new Mock<IMapper>();
            mapper
                .Setup(m => m.Map<TranslationServiceResponse>(It.IsAny<TranslationResponse>()))
                .Returns((TranslationResponse rabbitResponse) =>
                {
                    return new TranslationServiceResponse()
                    {
                        Translations = rabbitResponse.Translations.ToList()
                    };
                });
            mapper
                .Setup(m => m.Map<Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation.TranslationRequest>(It.IsAny<TranslationServiceRequest>()))
                .Returns((TranslationServiceRequest request) =>
                {
                    return new Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation.TranslationRequest()
                    {
                        Domain = request.Domain,
                        InputType = request.InputType.ToString(),
                        SourceLanguage = request.SourceLanguage,
                        TargetLanguage = request.TargetLanguage,
                        Text = request.Text
                    };
                });
            this.mapper = mapper.Object;
        }

        [Fact]
        public async Task ErrorIsReturned_WhenTimeoutDetected()
        {
            // --- Arrange

            var options = Options.Create(new ConfigurationSettings()
            {
                TranslationTimeout = TimeSpan.FromSeconds(10)
            });

            var mtRequestClient = new Mock<IRequestClient<Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation.TranslationRequest>>();
            mtRequestClient
                .Setup(m => m.Create(It.IsAny<Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation.TranslationRequest>(), It.IsAny<CancellationToken>(), It.IsAny<RequestTimeout>()))
                .Returns((Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation.TranslationRequest message, CancellationToken cancellationToken, RequestTimeout timeout) =>
                {
                    var consumeContext = new Mock<ConsumeContext<TranslationResponse>>();
                    consumeContext
                        .SetupGet(m => m.Message)
                        .Returns(new TranslationResponse()
                        {
                            StatusMessage = "",
                            StatusCode = 200,
                            Translations = message.Text
                        });

                    var requestHandler = new Mock<RequestHandle<Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation.TranslationRequest>>();
                    requestHandler
                        .Setup(m => m.GetResponse<TranslationResponse>(It.IsAny<bool>()))
                        .Throws(new TranslationTimeoutException(options.Value.TranslationTimeout));

                    return requestHandler.Object;
                });

            var service = new Tilde.MT.TranslationAPIService.Services.TranslationService(
                mapper,
                options,
                mtRequestClient.Object
            );
            // --- Act

            var exception = await Record.ExceptionAsync(async () =>
            {
                await service.Translate(translationRequest);
            });

            // --- Assert

            exception.Should().BeOfType<TranslationTimeoutException>();
        }

        [Fact]
        public async Task ErrorIsReturned_WhenTranslationWorkerError()
        {
            // --- Arrange

            var options = Options.Create(new ConfigurationSettings()
            {
                TranslationTimeout = TimeSpan.FromSeconds(10)
            });

            var mtRequestClient = new Mock<IRequestClient<Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation.TranslationRequest>>();
            mtRequestClient
                .Setup(m => m.Create(It.IsAny<Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation.TranslationRequest>(), It.IsAny<CancellationToken>(), It.IsAny<RequestTimeout>()))
                .Returns((Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation.TranslationRequest message, CancellationToken cancellationToken, RequestTimeout timeout) =>
                {
                    var responseMessage = new TranslationResponse()
                    {
                        StatusMessage = "status message",
                        StatusCode = 500,
                        Translations = message.Text
                    };

                    var consumeContext = new Mock<ConsumeContext<TranslationResponse>>();
                    consumeContext
                        .SetupGet(m => m.Message)
                        .Returns(responseMessage);

                    var requestHandler = new Mock<RequestHandle<Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation.TranslationRequest>>();
                    requestHandler
                        .Setup(m => m.GetResponse<TranslationResponse>(It.IsAny<bool>()))
                        .Throws(new TranslationWorkerException(responseMessage.StatusCode, responseMessage.StatusMessage));

                    return requestHandler.Object;
                });

            var service = new Tilde.MT.TranslationAPIService.Services.TranslationService(
                mapper,
                options,
                mtRequestClient.Object
            );
            // --- Act

            var exception = await Record.ExceptionAsync(async () =>
            {
                await service.Translate(translationRequest);
            });

            // --- Assert

            exception.Should().BeOfType<TranslationWorkerException>();
        }

        [Fact]
        public async Task TranslationIsReturned_WhenTranslationSuceeds()
        {
            // --- Arrange

            var options = Options.Create(new ConfigurationSettings()
            {
                TranslationTimeout = TimeSpan.FromSeconds(10)
            });

            var mtRequestClient = new Mock<IRequestClient<Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation.TranslationRequest>>();
            mtRequestClient
                .Setup(m => m.Create(It.IsAny<Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation.TranslationRequest>(), It.IsAny<CancellationToken>(), It.IsAny<RequestTimeout>()))
                .Returns((Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation.TranslationRequest message, CancellationToken cancellationToken, RequestTimeout timeout) =>
                {
                    var consumeContext = new Mock<ConsumeContext<TranslationResponse>>();
                    consumeContext
                        .SetupGet(m => m.Message)
                        .Returns(new TranslationResponse()
                        {
                            StatusMessage = "",
                            StatusCode = 200,
                            Translations = message.Text
                        });

                    var requestHandler = new Mock<RequestHandle<Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation.TranslationRequest>>();
                    requestHandler
                        .Setup(m => m.GetResponse<TranslationResponse>(It.IsAny<bool>()))
                        .Returns((bool send) =>
                        {
                            Response<TranslationResponse> response = new MassTransit.Clients.MessageResponse<TranslationResponse>(consumeContext.Object);
                            return Task.FromResult(response);
                        });

                    return requestHandler.Object;
                });

            var service = new Tilde.MT.TranslationAPIService.Services.TranslationService(
                mapper,
                options,
                mtRequestClient.Object
            );
            // --- Act

            var result = await service.Translate(translationRequest);

            // --- Assert

            result.Should().NotBeNull();
            result.Translations.Should().HaveCount(translationRequest.Text.Count);
        }
    }
}
