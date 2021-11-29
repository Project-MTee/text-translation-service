using AutoMapper;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
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
                    return Task.FromResult(new TranslationServiceResponse()
                    {
                        Translations = rabbitResponse.Translations
                    });
                });
            mapper
                .Setup(m => m.Map<Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation.TranslationRequest>(It.IsAny<TranslationServiceRequest>()))
                .Returns((TranslationServiceRequest request) =>
                {
                    Task.FromResult(new Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation.TranslationRequest()
                    {
                        Domain = request.Domain,
                        InputType = request.InputType.ToString(),
                        SourceLanguage = request.SourceLanguage,
                        TargetLanguage = request.TargetLanguage,
                        Text = request.Text
                    });
                });
            this.mapper = mapper.Object;
        }

        /*[Fact]
        public async Task TimeoutException_WhenTimeoutDetected()
        {
            // --- Arrange

            var options = Options.Create(new ConfigurationSettings()
            {
                TranslationTimeout = TimeSpan.Zero
            });

            var requestHandler = new Mock<RequestHandle<Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation.TranslationRequest>>();


            var mtRequestClient = new Mock<IRequestClient<Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation.TranslationRequest>>();
            mtRequestClient
                .Setup(m => m.Create(It.IsAny<Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation.TranslationRequest>(), It.IsAny<CancellationToken>(), It.IsAny<RequestTimeout>()))
                .Returns((Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation.TranslationRequest message, CancellationToken cancellationToken, RequestTimeout timeout) =>
                {
                    return Task.FromResult(requestHandler.Object);
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
        }*/
    }
}
