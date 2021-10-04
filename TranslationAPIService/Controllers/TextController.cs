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

        public TextController(
            ILogger<TextController> logger, 
            IMapper mapper,
            TranslationService translationService,
            DomainDetectionService domainDetectionService
        )
        {
            _logger = logger;
            _mapper = mapper;
            _translationService = translationService;
            _domainDetectionService = domainDetectionService;
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
        public async Task<ActionResult<Translation>> GetTranslation(Models.Translation.RequestTranslation request)
        {
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
                    _logger.LogError("Domain detection timed out");
                    return StatusCode(
                        (int)HttpStatusCode.GatewayTimeout,
                        new Translation()
                        {
                            Error = new Error()
                            {
                                Code = (int)HttpStatusCode.GatewayTimeout * 1000 + (int)ErrorSubCode.GatewayDomainDetectionTimedOut,
                                Message = "Domain detection timed out"
                            }
                        }
                    );
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Domain detection failed");

                    return StatusCode(
                        (int)HttpStatusCode.InternalServerError,
                        new Translation()
                        {
                            Error = new Error()
                            {
                                Code = (int)HttpStatusCode.InternalServerError * 1000 + (int)ErrorSubCode.GatewayDomainDetectionGeneric,
                                Message = ex.Message
                            }
                        }
                    );
                }
            }

            try
            {
                var response = await _translationService.Translate(translationMessage);

                if (response.StatusCode != (int)HttpStatusCode.OK)
                {
                    return StatusCode(
                        response.StatusCode,
                        new Translation()
                        {
                            Error = new Error()
                            {
                                Code = response.StatusCode + (int)ErrorSubCode.WorkerTranslationGeneric,
                                Message = response.Status
                            }
                        }
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
                return StatusCode(
                    (int)HttpStatusCode.GatewayTimeout,
                    new Translation()
                    {
                        Error = new Error()
                        {
                            Code = (int)HttpStatusCode.GatewayTimeout * 1000 + (int)ErrorSubCode.GatewayTranslationTimedOut,
                            Message = "Translation timed out"
                        }
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Translation failed");

                return StatusCode(
                    (int)HttpStatusCode.InternalServerError,
                    new Translation()
                    {
                        Error = new Error()
                        {
                            Code = (int)HttpStatusCode.InternalServerError * 1000 + (int)ErrorSubCode.GatewayTranslationGeneric,
                            Message = ex.Message
                        }
                    }
                );
            }
        }
    }
}
