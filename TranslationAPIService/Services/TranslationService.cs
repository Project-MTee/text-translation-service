using AutoMapper;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Tilde.MT.TranslationAPIService.Extensions.RabbitMQ;
using Tilde.MT.TranslationAPIService.Models;
using Tilde.MT.TranslationAPIService.Models.Configuration;
using Tilde.MT.TranslationAPIService.Models.Translation;

namespace Tilde.MT.TranslationAPIService.Services
{
    public class TranslationService
    {
        private readonly IBus _bus;
        private readonly ILogger _logger;
        private readonly ConfigurationServices _serviceConfiguration;
        private readonly IMapper _mapper;

        private readonly IRequestClient<Models.RabbitMQ.Translation.TranslationRequest> _translationClient;
        private readonly IRequestClient<Models.RabbitMQ.DomainDetection.DomainDetectionRequest> _domainDetectionClient;

        private readonly ISendEndpointProvider _sendEndpointProvider;

        public TranslationService(
            ILogger<TranslationService> logger,
            IBus bus,
            IOptions<ConfigurationServices> configurationSettings,
            IMapper mapper,
            ISendEndpointProvider sendEndpointProvider
        )
        {
            _bus = bus;
            
            _logger = logger;
            _serviceConfiguration = configurationSettings.Value;
            _mapper = mapper;

            var translationServiceAddress = new Uri($"exchange:{_serviceConfiguration.RabbitMQ.TranslationExchangeName}?type=direct");
            _translationClient = _bus.CreateRequestClient<Models.RabbitMQ.Translation.TranslationRequest>(translationServiceAddress);
            var domainDetectionServiceAdress = new Uri($"exchange:{_serviceConfiguration.RabbitMQ.LanguageDetectionExchangeName}?type=direct");
            _domainDetectionClient = _bus.CreateRequestClient<Models.RabbitMQ.DomainDetection.DomainDetectionRequest>(domainDetectionServiceAdress);

            _sendEndpointProvider = sendEndpointProvider;
        }
        public async Task<ServiceResponse<Translation>> Translate(RequestTranslation translationRequest)
        {
            var response = new ServiceResponse<Translation>();
            try
            {
                var rabbitMQMessage = _mapper.Map<Models.RabbitMQ.Translation.TranslationRequest>(translationRequest);

                /*if (string.IsNullOrEmpty(rabbitMQMessage.Domain))
                {
                    _logger.LogDebug("Domain not provided, fetch from Domain detection");

                    var domainDetectionMessage = new Models.RabbitMQ.DomainDetection.DomainDetectionRequest()
                    {
                        SourceLanguage = rabbitMQMessage.SourceLanguage,
                        Text = rabbitMQMessage.Text
                    };

                    var domainResponse = await _domainDetectionClient.GetResponse<Models.RabbitMQ.DomainDetection.DomainDetectionResponse>(domainDetectionMessage);
                    rabbitMQMessage.Domain = domainResponse.Message.Domain;

                    _logger.LogDebug($"Detected domain: {rabbitMQMessage.Domain}");
                }

                _logger.LogDebug("Request translation from MT system");

                using var request = _translationClient.Create(translationRequest);
                request.UseExecute(x => x.AddReplyToProperty());
                var translationResponse = await request.GetResponse<Models.RabbitMQ.Translation.TranslationResponse>();

                response.Data = _mapper.Map<Translation>(translationResponse.Message);*/

                var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"exchange:{_serviceConfiguration.RabbitMQ.TranslationExchangeName}?type=direct"));


                await endpoint.Send<Models.RabbitMQ.Translation.TranslationRequest>(
                    rabbitMQMessage,
                    context => {
                        context.CorrelationId = new Guid("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");// Guid.NewGuid();
                    }
                );


                _logger.LogDebug("Translation received");
            }
            catch (RequestTimeoutException ex)
            {
                _logger.LogError(ex, "Translation timed out");
                response.Error = new Error()
                {
                    Code = 400,
                    Message = "Translation timed out"
                };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to translate text");
                response.Error = new Error()
                {
                    Code = 500,
                    Message = "Unknown error"
                };
            }

            return response;
        }
    }
}
