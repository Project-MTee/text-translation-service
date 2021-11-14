using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
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
        /// <param name="detectionRequest"></param>
        /// <exception cref="RequestTimeoutException">Message is not being received in configured timeout period via RabbitMQ</exception>
        /// <returns></returns>
        public async Task<DomainDetectionResponse> Detect(DomainDetectionRequest detectionRequest)
        {
            using var request = _requestClient.Create(detectionRequest, timeout: _configurationSettings.DomainDetectionTimeout);
            request.UseExecute(x => x.AddRequestHeaders<DomainDetectionResponse>());
            var translationResponse = await request.GetResponse<DomainDetectionResponse>();

            return translationResponse.Message;
        }
    }
}
