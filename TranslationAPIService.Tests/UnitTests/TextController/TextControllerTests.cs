using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tilde.MT.TranslationAPIService.Exceptions.DomainDetection;
using Tilde.MT.TranslationAPIService.Exceptions.LanguageDirection;
using Tilde.MT.TranslationAPIService.Exceptions.Translation;
using Tilde.MT.TranslationAPIService.Models.DTO.Translation;
using Tilde.MT.TranslationAPIService.Models.Errors;
using Tilde.MT.TranslationAPIService.Services;
using Xunit;

namespace TranslationAPIService.Tests.UnitTests.TextController
{
    public class TextControllerTests
    {
        private readonly TranslationRequest translationRequest;
        private readonly IMapper mapper;

        public TextControllerTests()
        {
            translationRequest = new TranslationRequest()
            {
                Domain = "finance",
                InputType = Tilde.MT.TranslationAPIService.Enums.TranslationType.plain,
                SourceLanguage = "en",
                TargetLanguage = "fr",
                Text = new List<string>
                {
                    "text message",
                    "other text message"
                }
            };
            var mapperMock = new Mock<IMapper>();
            mapperMock
                .Setup(m => m.Map<TranslationServiceRequest>(It.IsAny<TranslationRequest>()))
                .Returns((TranslationRequest source) =>
                {
                    var result = new TranslationServiceRequest()
                    {
                        Domain = source.Domain,
                        InputType = source.InputType,
                        SourceLanguage = source.SourceLanguage,
                        TargetLanguage = source.TargetLanguage,
                        Text = source.Text
                    };

                    return result;
                });

            mapper = mapperMock.Object;
        }

        #region Helpers

        private static ITranslationService MockSuccessTranslationService()
        {
            var translationService = new Mock<ITranslationService>();
            translationService
                .Setup(m => m.Translate(It.IsAny<TranslationServiceRequest>()))
                .Returns((TranslationServiceRequest translationRequest) =>
                {
                    return Task.FromResult(
                        new TranslationServiceResponse()
                        {
                            Translations = translationRequest.Text
                        }
                    );
                });

            return translationService.Object;
        }

        private static IDomainDetectionService MockSuccessDomainDetection()
        {
            var domainDetectionService = new Mock<IDomainDetectionService>();
            domainDetectionService
                .Setup(m => m.Detect(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns((string sourceLanguage, List<string> text) =>
                {
                    return Task.FromResult("finance");
                });

            return domainDetectionService.Object;
        }

        #endregion

        #region Domain detection tests 

        [Fact]
        public async Task DomainIsSetToDefault_WhenNoDomainAndDomainDetectionTimeout()
        {
            // --- Arrange

            var message = translationRequest with { Domain = null };

            var domainDetectionService = new Mock<IDomainDetectionService>();
            domainDetectionService
                .Setup(m => m.Detect(It.IsAny<string>(), It.IsAny<List<string>>()))
                .ThrowsAsync(new DomainDetectionTimeoutException(TimeSpan.Zero));

            var controller = new Tilde.MT.TranslationAPIService.Controllers.TextController(
                Mock.Of<ILogger<Tilde.MT.TranslationAPIService.Controllers.TextController>>(),
                mapper,
                MockSuccessTranslationService(),
                domainDetectionService.Object,
                Mock.Of<ILanguageDirectionService>()
            );
            // --- Act

            var result = await controller.GetTranslation(message);

            // --- Assert

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var translationResult = okResult.Value.Should().BeOfType<Translation>().Subject;

            // Default domain should be set
            translationResult.Domain.Should().Be("general");
        }

        [Fact]
        public async Task DomainIsSetToDefault_WhenNoDomainAndDetectionGeneralException()
        {
            // --- Arrange

            var message = translationRequest with { Domain = null };

            var domainDetectionService = new Mock<IDomainDetectionService>();
            domainDetectionService
                .Setup(m => m.Detect(It.IsAny<string>(), It.IsAny<List<string>>()))
                .ThrowsAsync(new Exception());

            var controller = new Tilde.MT.TranslationAPIService.Controllers.TextController(
                Mock.Of<ILogger<Tilde.MT.TranslationAPIService.Controllers.TextController>>(),
                mapper,
                MockSuccessTranslationService(),
                domainDetectionService.Object,
                Mock.Of<ILanguageDirectionService>()
            );
            // --- Act

            var result = await controller.GetTranslation(message);

            // --- Assert

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var translationResult = okResult.Value.Should().BeOfType<Translation>().Subject;

            // Default domain should be set
            translationResult.Domain.Should().Be("general");
        }

        #endregion

        #region Translation tests

        [Fact]
        public async Task TranslationSuccess_WhenTranslationSuceeds()
        {
            // --- Arrange

            var message = translationRequest with { Domain = null };

            var controller = new Tilde.MT.TranslationAPIService.Controllers.TextController(
                Mock.Of<ILogger<Tilde.MT.TranslationAPIService.Controllers.TextController>>(),
                mapper,
                MockSuccessTranslationService(),
                MockSuccessDomainDetection(),
                Mock.Of<ILanguageDirectionService>()
            );
            // --- Act

            var result = await controller.GetTranslation(message);

            // --- Assert

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var translationResult = okResult.Value.Should().BeOfType<Translation>().Subject;

            // Domain is detected or it is default
            translationResult.Domain.Should().NotBeNullOrEmpty();

            // Segment count in translation should not vary, they should be the same.
            translationRequest.Text.Count.Should().Be(message.Text.Count);
        }

        [Fact]
        public async Task TranslationFails_WhenTranslationTimeout()
        {
            // --- Arrange

            var message = translationRequest with { Domain = null };

            var translationService = new Mock<ITranslationService>();
            translationService
                .Setup(m => m.Translate(It.IsAny<TranslationServiceRequest>()))
                .ThrowsAsync(new TranslationTimeoutException(TimeSpan.Zero));

            var controller = new Tilde.MT.TranslationAPIService.Controllers.TextController(
                Mock.Of<ILogger<Tilde.MT.TranslationAPIService.Controllers.TextController>>(),
                mapper,
                translationService.Object,
                MockSuccessDomainDetection(),
                Mock.Of<ILanguageDirectionService>()
            );
            // --- Act

            var result = await controller.GetTranslation(message);

            // --- Assert

            var okResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            var translationResult = okResult.Value.Should().BeOfType<APIError>().Subject;

            translationResult.Error.Should().NotBeNull();
            translationResult.Error.Code.Should().Be(504003);
            translationResult.Error.Message.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task TranslationFails_WhenTranslationGeneralException()
        {
            // --- Arrange

            var message = translationRequest with { Domain = null };

            var translationService = new Mock<ITranslationService>();
            translationService
                .Setup(m => m.Translate(It.IsAny<TranslationServiceRequest>()))
                .ThrowsAsync(new Exception());

            var controller = new Tilde.MT.TranslationAPIService.Controllers.TextController(
                Mock.Of<ILogger<Tilde.MT.TranslationAPIService.Controllers.TextController>>(),
                mapper,
                translationService.Object,
                MockSuccessDomainDetection(),
                Mock.Of<ILanguageDirectionService>()
            );
            // --- Act

            var result = await controller.GetTranslation(message);

            // --- Assert

            var okResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            var translationResult = okResult.Value.Should().BeOfType<APIError>().Subject;

            translationResult.Error.Should().NotBeNull();
            translationResult.Error.Code.Should().Be(500004);
            translationResult.Error.Message.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task TranslationFails_WhenTranslationWorkerError()
        {
            // --- Arrange

            var translationWorkerErrorCode = 777;
            var translationWorkerErrorMessage = $"{translationWorkerErrorCode} error happened here";

            var message = translationRequest with { Domain = null };

            var translationService = new Mock<ITranslationService>();
            translationService
                .Setup(m => m.Translate(It.IsAny<TranslationServiceRequest>()))
                .ThrowsAsync(new TranslationWorkerException(translationWorkerErrorCode, translationWorkerErrorMessage));

            var controller = new Tilde.MT.TranslationAPIService.Controllers.TextController(
                Mock.Of<ILogger<Tilde.MT.TranslationAPIService.Controllers.TextController>>(),
                mapper,
                translationService.Object,
                MockSuccessDomainDetection(),
                Mock.Of<ILanguageDirectionService>()
            );
            // --- Act

            var result = await controller.GetTranslation(message);

            // --- Assert

            var okResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            var translationResult = okResult.Value.Should().BeOfType<APIError>().Subject;

            translationResult.Error.Should().NotBeNull();
            translationResult.Error.Code.Should().Be(translationWorkerErrorCode * 1000 + 5);
            translationResult.Error.Message.Should().Be(translationWorkerErrorMessage);
        }

        #endregion

        #region Language direction tests

        [Fact]
        public async Task TranslationFails_WhenLanguageDirectionNotFound()
        {
            // --- Arrange

            var message = translationRequest with { Domain = null };

            var languageDirectionService = new Mock<ILanguageDirectionService>();
            languageDirectionService
                .Setup(m => m.Validate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string domain, string sourceLanguage, string targetLanguage) =>
                {
                    throw new LanguageDirectionNotFoundException(domain, sourceLanguage, targetLanguage);
                });
            var controller = new Tilde.MT.TranslationAPIService.Controllers.TextController(
                Mock.Of<ILogger<Tilde.MT.TranslationAPIService.Controllers.TextController>>(),
                mapper,
                MockSuccessTranslationService(),
                MockSuccessDomainDetection(),
                languageDirectionService.Object
            );
            // --- Act

            var result = await controller.GetTranslation(message);

            // --- Assert

            var okResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            var translationResult = okResult.Value.Should().BeOfType<APIError>().Subject;

            translationResult.Error.Should().NotBeNull();
            translationResult.Error.Code.Should().Be(404006);
            translationResult.Error.Message.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task TranslationFails_WhenLanguageDirectionReadException()
        {
            // --- Arrange

            var message = translationRequest with { Domain = null };

            var languageDirectionService = new Mock<ILanguageDirectionService>();
            languageDirectionService
                .Setup(m => m.Validate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new LanguageDirectionReadException());

            var controller = new Tilde.MT.TranslationAPIService.Controllers.TextController(
                Mock.Of<ILogger<Tilde.MT.TranslationAPIService.Controllers.TextController>>(),
                mapper,
                MockSuccessTranslationService(),
                MockSuccessDomainDetection(),
                languageDirectionService.Object
            );
            // --- Act

            var result = await controller.GetTranslation(message);

            // --- Assert

            var okResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            var translationResult = okResult.Value.Should().BeOfType<APIError>().Subject;

            translationResult.Error.Should().NotBeNull();
            translationResult.Error.Code.Should().Be(500007);
            translationResult.Error.Message.Should().NotBeNullOrEmpty();
        }

        #endregion
    }
}
