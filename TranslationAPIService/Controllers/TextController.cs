﻿using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tilde.MT.TranslationAPIService.Controllers;
using Tilde.MT.TranslationAPIService.Enums;
using Tilde.MT.TranslationAPIService.Exceptions.LanguageDirection;
using Tilde.MT.TranslationAPIService.Exceptions.Translation;
using Tilde.MT.TranslationAPIService.Models.DTO.Translation;
using Tilde.MT.TranslationAPIService.Models.Errors;
using Tilde.MT.TranslationAPIService.Services;

namespace Tilde.MT.TranslationAPIService.TranslationAPI.Controllers
{
    /// <summary>
    /// API for text translation
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class TextController : BaseController
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
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Missing or incorrect parameters", Type = typeof(APIError))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "An unexpected error occured", Type = typeof(APIError))]
        [SwaggerResponse((int)HttpStatusCode.GatewayTimeout, Description = "Request timed out", Type = typeof(APIError))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Language direction is not found", Type = typeof(APIError))]
        [SwaggerResponse((int)HttpStatusCode.RequestEntityTooLarge, Description = "Maximum text size limit reached for the request", Type = typeof(APIError))]
        public async Task<ActionResult<Translation>> GetTranslation(TranslationRequest request)
        {
            try
            {
                await _languageDirectionService.Validate(request.Domain, request.SourceLanguage, request.TargetLanguage);
            }
            catch (LanguageDirectionNotFoundException ex)
            {
                _logger.LogError(ex, "Language direction not found");

                return FormatAPIError(HttpStatusCode.NotFound, ErrorSubCode.GatewayLanguageDirectionNotFound);
            }
            catch (LanguageDirectionReadException ex)
            {
                _logger.LogError(ex, "Exception loading language directions");

                return FormatAPIError(HttpStatusCode.InternalServerError, ErrorSubCode.GatewayLanguageDirectionGeneric);
            }

            var translationMessage = _mapper.Map<Models.DTO.Translation.TranslationServiceRequest>(request);

            if (string.IsNullOrEmpty(request.Domain))
            {
                try
                {
                    _logger.LogDebug("Request domain detection, domain not provided");
                    var detectedDomain = await _domainDetectionService.Detect(request.SourceLanguage, request.Text);

                    translationMessage.Domain = detectedDomain;
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
            catch (TranslationWorkerException ex)
            {
                _logger.LogError($"Translation failed");

                return FormatAPIError(
                    HttpStatusCode.InternalServerError,
                    ErrorSubCode.WorkerTranslationGeneric,
                    message: ex.StatusMessage,
                    messageStatusCode: (HttpStatusCode)ex.StatusCode
                );
            }
            catch (TranslationTimeoutException ex)
            {
                _logger.LogError(ex, "Translation timed out");

                return FormatAPIError(HttpStatusCode.GatewayTimeout, ErrorSubCode.GatewayTranslationTimedOut);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Translation failed");

                return FormatAPIError(HttpStatusCode.InternalServerError, ErrorSubCode.GatewayTranslationGeneric);
            }
        }
    }
}
