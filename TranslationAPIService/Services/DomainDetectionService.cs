using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tilde.MT.TranslationAPIService.Extensions.MassTransit;
using Tilde.MT.TranslationAPIService.Interfaces.Services;
using Tilde.MT.TranslationAPIService.Models.Configuration;
using Tilde.MT.TranslationAPIService.Models.RabbitMQ.DomainDetection;

namespace Tilde.MT.TranslationAPIService.Services
{
    public class DomainDetectionService : IDomainDetectionService
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

        /// <inheritdoc/>
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
                throw new TimeoutException($"Exception timed out in '{_configurationSettings.DomainDetectionTimeout}'");
            }
        }
    }
}
