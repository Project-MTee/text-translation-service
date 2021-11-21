using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tilde.MT.TranslationAPIService.Exceptions.DomainDetection;
using Tilde.MT.TranslationAPIService.Extensions.MassTransit;
using Tilde.MT.TranslationAPIService.Models.Configuration;
using Tilde.MT.TranslationAPIService.Models.RabbitMQ.DomainDetection;

namespace Tilde.MT.TranslationAPIService.Services
{
    public class DomainDetectionService
    {
        private readonly ConfigurationSettings _configurationSettings;
        private readonly IRequestClient<DomainDetectionRequest> _requestClient;

        public DomainDetectionService(
            IOptions<ConfigurationSettings> configurationSettings,
            IRequestClient<DomainDetectionRequest> requestClient
        )
        {
            _configurationSettings = configurationSettings.Value;
            _requestClient = requestClient;
        }

        /// <summary>
        /// Detect domain using domain detection worker
        /// </summary>
        /// <exception cref="DomainDetectionTimeoutException">Message is not being received in configured timeout period via RabbitMQ</exception>
        /// <returns></returns>
        public async Task<string> Detect(string sourceLanguage, List<string> text)
        {
            var detectionRequest = new DomainDetectionRequest()
            {
                SourceLanguage = sourceLanguage,
                Text = text
            };

            try
            {
                using var request = _requestClient.Create(detectionRequest, timeout: _configurationSettings.DomainDetectionTimeout);
                request.UseExecute(x => x.AddRequestHeaders<DomainDetectionResponse>());
                var translationResponse = await request.GetResponse<DomainDetectionResponse>();

                return translationResponse.Message.Domain;
            }
            catch (RequestTimeoutException)
            {
                throw new DomainDetectionTimeoutException();
            }
        }
    }
}
