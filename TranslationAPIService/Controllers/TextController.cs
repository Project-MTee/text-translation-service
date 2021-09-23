using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tilde.MT.TranslationAPIService.Models;
using Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation;

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
        private readonly IRequestClient<Models.RabbitMQ.Translation.TranslationRequest> _translationClient;
        private readonly IRequestClient<Models.RabbitMQ.Translation.TranslationRequest> _domainDetectionClient;
        private readonly IMapper _mapper;
        private readonly IBus _bus;

        public TextController(
            ILogger<TextController> logger, 
            IMapper mapper,
            IRequestClient<Models.RabbitMQ.Translation.TranslationRequest> translationClient,
            IRequestClient<Models.RabbitMQ.Translation.TranslationRequest> domainDetectionClient,
            IBus bus
        )
        {
            _logger = logger;
            _mapper = mapper;
            _translationClient = translationClient;
            _domainDetectionClient = domainDetectionClient;
            _bus = bus;
        }

        /// <summary>
        /// Translate text
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Translation>> GetTranslation(Models.TranslationRequest request)
        {
            /*var rabbitMQModel = _mapper.Map<Models.RabbitMQ.Translation.TranslationRequest>(request);

            if (string.IsNullOrEmpty(request.Domain))
            {
                _logger.LogDebug("Domain not provided, fetch from Domain detection service");

                var domainRequestModel = new Models.RabbitMQ.DomainDetection.DomainDetectionRequest()
                {
                    SourceLanguage = request.SourceLanguage,
                    Text = request.Text
                };
                var domainResponse = await _translationClient.GetResponse<Models.RabbitMQ.DomainDetection.DomainDetectionResponse>(domainRequestModel);
                rabbitMQModel.Domain = domainResponse.Message.Domain;
            }*/
            /*
            _logger.LogDebug("Request translation from MT system");

            var serviceAddress = new Uri("rabbitmq://localhost/check-order-status");
            var client = _bus.CreateRequestClient<Models.RabbitMQ.Translation.TranslationRequest>(serviceAddres);

            var response = await client.GetResponse<TranslationResponse>(rabbitMQModel);
            */

            /*var response = await _translationClient.GetResponse<TranslationResponse>(rabbitMQModel);
            
            var responseModel = _mapper.Map<Models.Translation>(response.Message);

            _logger.LogDebug("Translation received");

            return Ok(responseModel);*/


            var dummyTranslations = new Translation()
            {
                Domain = request.Domain ?? "translated domain",
                Translations = request.Text.Select(item => new TranslationItem()
                {
                    Translation = "_translated_    " + item
                }).ToList()
            };

            return dummyTranslations;
        }
    }
}
