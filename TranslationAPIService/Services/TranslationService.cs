using AutoMapper;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tilde.MT.TranslationAPIService.Extensions.RabbitMQ;
using Tilde.MT.TranslationAPIService.Models;
using Tilde.MT.TranslationAPIService.Models.Configuration;
using Tilde.MT.TranslationAPIService.Models.Translation;

namespace Tilde.MT.TranslationAPIService.Services
{
    public class TranslationService
    {
        //private readonly IBus _bus;
        private readonly ILogger _logger;
        private readonly ConfigurationServices _serviceConfiguration;
        private readonly IMapper _mapper;
        private readonly RequestClient<Models.RabbitMQ.Translation.TranslationRequest, Models.RabbitMQ.Translation.TranslationResponse> _translationClient;
        private readonly RequestClient<Models.RabbitMQ.DomainDetection.DomainDetectionRequest, Models.RabbitMQ.DomainDetection.DomainDetectionResponse> _domainDetectionClient;
        public TranslationService(
            ILogger<TranslationService> logger,
            IOptions<ConfigurationServices> configurationSettings,
            IMapper mapper,
            RequestClient<Models.RabbitMQ.Translation.TranslationRequest, Models.RabbitMQ.Translation.TranslationResponse> translationClient,
            RequestClient<Models.RabbitMQ.DomainDetection.DomainDetectionRequest, Models.RabbitMQ.DomainDetection.DomainDetectionResponse> domainDetectionClient
        )
        {
            _logger = logger;
            _serviceConfiguration = configurationSettings.Value;
            _mapper = mapper;

            _translationClient = translationClient;
            _domainDetectionClient = domainDetectionClient;
        }
        public async Task<ServiceResponse<Translation>> Translate(RequestTranslation translationRequest)
        {
            var response = new ServiceResponse<Translation>();
            try
            {
                var rabbitMQMessage = _mapper.Map<Models.RabbitMQ.Translation.TranslationRequest>(translationRequest);

                if (string.IsNullOrEmpty(rabbitMQMessage.Domain))
                {
                    _logger.LogDebug("Domain not provided, fetch from Domain detection");

                    var domainDetectionMessage = new Models.RabbitMQ.DomainDetection.DomainDetectionRequest()
                    {
                        SourceLanguage = rabbitMQMessage.SourceLanguage,
                        Text = rabbitMQMessage.Text
                    };

                    var domainRoutingKey = GetDomainDetectionRoutingKey(domainDetectionMessage);
                    var domainResponse = await _domainDetectionClient.GetResponse(domainDetectionMessage, domainRoutingKey, "domain-detection", TimeSpan.FromSeconds(10));
                    rabbitMQMessage.Domain = domainResponse.Domain;

                    _logger.LogDebug($"Detected domain: {rabbitMQMessage.Domain}");
                }

                _logger.LogDebug("Request translation from MT system");

                var translationRoutingKey = GetTranslationRoutingKey(rabbitMQMessage);
                var translationResponse = await _translationClient.GetResponse(rabbitMQMessage, translationRoutingKey, "translation", TimeSpan.FromSeconds(10));

                _logger.LogDebug("Translation received");

                response.Data = new Translation()
                {
                    Domain = rabbitMQMessage.Domain,
                    Translations = translationResponse.Translations.Select(item => new TranslationItem() { 
                        Translation = item 
                    }).ToList()
                };
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Translation timed out");
                response.Error = new Error()
                {
                    Code = (int)HttpStatusCode.GatewayTimeout,
                    Message = "Translation timed out"
                };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to translate text");
                response.Error = new Error()
                {
                    Code = (int)HttpStatusCode.InternalServerError,
                    Message = "Unknown error"
                };
            }

            return response;
        }

        private string GetTranslationRoutingKey(Models.RabbitMQ.Translation.TranslationRequest item)
        {
            return $"translation.{item.SourceLanguage}.{item.TargetLanguage}.{item.Domain}.plain";
        }

        private string GetDomainDetectionRoutingKey(Models.RabbitMQ.DomainDetection.DomainDetectionRequest item)
        {
            return $"domain-detection.{item.SourceLanguage}";
        }
    }
}
