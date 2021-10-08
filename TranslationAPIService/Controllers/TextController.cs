using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tilde.MT.TranslationAPIService.Enums;
using Tilde.MT.TranslationAPIService.Models;
using Tilde.MT.TranslationAPIService.Models.Translation;
using Tilde.MT.TranslationAPIService.Services;

namespace Tilde.MT.TranslationAPIService.TranslationAPI.Controllers
{
    /// <summary>
    /// API for text translation
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class TextController : ControllerBase
    {
        private readonly ILogger<TextController> _logger;
        private readonly IMapper _mapper;
        private readonly TranslationService _translationService;
        private readonly DomainDetectionService _domainDetectionService;
        private readonly LanguageDirectionService _languageDirectionService;

        public TextController(
            ILogger<TextController> logger, 
            IMapper mapper,
            TranslationService translationService,
            DomainDetectionService domainDetectionService,
            LanguageDirectionService languageDirectionService
        )
        {
            _logger = logger;
            _mapper = mapper;
            _translationService = translationService;
            _domainDetectionService = domainDetectionService;
            _languageDirectionService = languageDirectionService;
        }

        /// <summary>
        /// Translate text
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(Translation))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Missing or incorrect parameters", Type =typeof(APIError))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "An unexpected error occured", Type=typeof(APIError))]
        [SwaggerResponse((int)HttpStatusCode.GatewayTimeout, Description = "Request timed out", Type=typeof(APIError))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Language direction is not found", Type = typeof(APIError))]
        [SwaggerResponse((int)HttpStatusCode.RequestEntityTooLarge, Description = "Maximum text size limit reached for the request", Type = typeof(APIError))]
        public async Task<ActionResult<Translation>> GetTranslation(Models.Translation.RequestTranslation request)
        {
            var languageDirections = await _languageDirectionService.Read();

            if (languageDirections == null)
            {
                return FormatTranslationError(
                    HttpStatusCode.InternalServerError,
                    ErrorSubCode.GatewayLanguageDirectionGeneric,
                    "Failed to verify language direction"
                );
            }

            // check if language direction exists.
            var languageDirectionInSettings = languageDirections.Where(item =>
            {
                var languageMatches = item.SourceLanguage == request.SourceLanguage &&
                    item.TargetLanguage == request.TargetLanguage;

                var domainMatches = string.IsNullOrEmpty(request.Domain) || item.Domain == request.Domain;

                return domainMatches && languageMatches;
            });

            if (!languageDirectionInSettings.Any())
            {
                return FormatTranslationError(
                    HttpStatusCode.NotFound,
                    ErrorSubCode.GatewayLanguageDirectionNotFound,
                    "Language direction is not found"
                );
            }

            var translationMessage = _mapper.Map<Models.RabbitMQ.Translation.TranslationRequest>(request);

            if (string.IsNullOrEmpty(request.Domain))
            {
                try
                {
                    _logger.LogDebug("Request domain detection, domain not provided");
                    var detectedDomain = await _domainDetectionService.Detect(new Models.RabbitMQ.DomainDetection.DomainDetectionRequest()
                    {
                        SourceLanguage = request.SourceLanguage,
                        Text = request.Text
                    });

                    translationMessage.Domain = detectedDomain.Domain;
                }
                catch (RequestTimeoutException)
                {
                    _logger.LogWarning("Domain detection timed out");

                    translationMessage.Domain = "general";

                    /*return FormatTranslationError(
                        HttpStatusCode.GatewayTimeout,
                        ErrorSubCode.GatewayDomainDetectionTimedOut,
                        "Domain detection timed out"
                    );*/
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Domain detection failed");

                    translationMessage.Domain = "general";

                    /*
                    return FormatTranslationError(
                        HttpStatusCode.InternalServerError,
                        ErrorSubCode.GatewayDomainDetectionGeneric,
                        "Domain detection failed due to unkown reason"
                    );*/
                }
            }

            try
            {
                var response = await _translationService.Translate(translationMessage);

                if ((HttpStatusCode)response.StatusCode != HttpStatusCode.OK)
                {
                    _logger.LogError($"Translation failed, worker error code: {response.StatusCode}, error status: {response.Status}");

                    return FormatTranslationError(
                        HttpStatusCode.InternalServerError,
                        ErrorSubCode.WorkerTranslationGeneric,
                        response.Status,
                        messageStatusCode: (HttpStatusCode)response.StatusCode
                    );
                }
                else
                {
                    var translationResponse = new Translation()
                    {
                        Domain = translationMessage.Domain,
                        Translations = response.Translations.Select(item => new TranslationItem()
                        {
                            Translation = item
                        }).ToList()
                    };
                    return Ok(translationResponse);
                }
            }
            catch (RequestTimeoutException)
            {
                _logger.LogError("Translation timed out");

                return FormatTranslationError(
                    HttpStatusCode.GatewayTimeout,
                    ErrorSubCode.GatewayTranslationTimedOut,
                    "Translation timed out"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Translation failed");

                return FormatTranslationError(
                    HttpStatusCode.InternalServerError,
                    ErrorSubCode.GatewayTranslationGeneric,
                    "Translation failed due to unkown reason"
                );
            }
        }

        private ActionResult<Translation> FormatTranslationError(HttpStatusCode status, ErrorSubCode subcode, string message, HttpStatusCode? messageStatusCode = null)
        {
            return StatusCode(
                (int)status,
                new APIError()
                {
                    Error = new Error()
                    {
                        Code = (int)(messageStatusCode ?? status) * 1000 + (int)subcode,
                        Message = message
                    }
                }
            );
        }
    }
}
