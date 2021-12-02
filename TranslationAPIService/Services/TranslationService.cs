using AutoMapper;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tilde.MT.TranslationAPIService.Exceptions.Translation;
using Tilde.MT.TranslationAPIService.Extensions.MassTransit;
using Tilde.MT.TranslationAPIService.Interfaces.Services;
using Tilde.MT.TranslationAPIService.Models.Configuration;
using Tilde.MT.TranslationAPIService.Models.DTO.Translation;
using Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation;

namespace Tilde.MT.TranslationAPIService.Services
{
    public class TranslationService: ITranslationService
    {
        private readonly ConfigurationSettings _configurationSettings;
        private readonly IRequestClient<Models.RabbitMQ.Translation.TranslationRequest> _requestClient;
        private readonly IMapper _mapper;

        public TranslationService(
            IMapper mapper,
            IOptions<ConfigurationSettings> configurationSettings,
            IRequestClient<Models.RabbitMQ.Translation.TranslationRequest> requestClient
        )
        {
            _configurationSettings = configurationSettings.Value;
            _requestClient = requestClient;
            _mapper = mapper;
        }

        /// <summary>
        /// Translate text using MT worker 
        /// </summary>
        /// <param name="translationRequest"></param>
        /// <exception cref="TranslationTimeoutException">Message is not being received in configured timeout period via RabbitMQ</exception>
        /// <exception cref="TranslationWorkerException">Translation worker returned error response</exception>
        /// <returns></returns>
        public async Task<TranslationServiceResponse> Translate(TranslationServiceRequest translationRequest)
        {
            var originalMessage = _mapper.Map<Models.RabbitMQ.Translation.TranslationRequest>(translationRequest);

            var chunkedMessages = originalMessage.Text.Chunk(_configurationSettings.TranslationRequestSegmentCount).Select(segments => originalMessage with { Text = segments }) ;

            var response = new TranslationServiceResponse()
            {
                Translations = new List<string>()
            };

            foreach (var chunkedMessage in chunkedMessages)
            {
                try
                {
                    using var request = _requestClient.Create(chunkedMessage, timeout: _configurationSettings.TranslationTimeout);
                    request.UseExecute(x => x.AddRequestHeaders<TranslationResponse>());
                    var translationResponse = await request.GetResponse<TranslationResponse>();

                    if (translationResponse.Message.StatusCode != 200)
                    {
                        throw new TranslationWorkerException(translationResponse.Message.StatusCode, translationResponse.Message.StatusMessage);
                    }

                    var chunkResponse = _mapper.Map<TranslationServiceResponse>(translationResponse.Message);

                    response.Translations.AddRange(chunkResponse.Translations);
                }
                catch (RequestTimeoutException)
                {
                    // Convert exception to service specific non RabbitMQ exception
                    throw new TranslationTimeoutException(_configurationSettings.TranslationTimeout);
                }
            }

            return response;
        }
    }
}
