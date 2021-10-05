using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tilde.MT.TranslationAPIService.Enums;
using Tilde.MT.TranslationAPIService.Models;
using Tilde.MT.TranslationAPIService.Models.Configuration;
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
        private readonly ConfigurationSettings _configurationSettings;

        public TextController(
            ILogger<TextController> logger, 
            IMapper mapper,
            TranslationService translationService,
            DomainDetectionService domainDetectionService,
            IOptions<ConfigurationSettings> configurationSettings
        )
        {
            _logger = logger;
            _mapper = mapper;
            _translationService = translationService;
            _domainDetectionService = domainDetectionService;
            _configurationSettings = configurationSettings.Value;
        }

        /// <summary>
        /// Translate text
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(Translation))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "An unexpected error occured. See the response for more details.", Type=typeof(APIResponse))]
        [SwaggerResponse((int)HttpStatusCode.GatewayTimeout, Description = "Request timed out.", Type=typeof(APIResponse))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Language direction is not found", Type = typeof(APIResponse))]
        public async Task<ActionResult<Translation>> GetTranslation(Models.Translation.RequestTranslation request)
        {
            // check if language direction exists.
            var languageDirectionInSettings = _configurationSettings.LanguageDirections.Find(item => {
                var languageMatches = item.SourceLanguage == request.SourceLanguage &&
                    item.TargetLanguage == request.TargetLanguage;

                var domainMatches = string.IsNullOrEmpty(request.Domain) ? true: item.Domain == request.Domain;

                return domainMatches && languageMatches;
            });
            if (languageDirectionInSettings == null)
            {
                return FormatTranslationError(
                    HttpStatusCode.NotFound,
                    ErrorSubCode.GatewayLanguageDirectionNotFound,
                    "Language direction is not found"
                );
            }

            var translationMessage = _mapper.Map<Models.RabbitMQ.Translation.TranslationRequest>(request);
            translationMessage.InputType = TranslationType.plain.ToString();
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

                    return FormatTranslationError(
                        HttpStatusCode.GatewayTimeout,
                        ErrorSubCode.GatewayDomainDetectionTimedOut,
                        "Domain detection timed out"
                    );
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Domain detection failed");

                    return FormatTranslationError(
                        HttpStatusCode.InternalServerError,
                        ErrorSubCode.GatewayDomainDetectionGeneric,
                        "Domain detection failed due to unkown reason"
                    );
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
                _logger.LogWarning("Translation timed out");

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
                new Translation()
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
