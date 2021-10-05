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
        private readonly ILogger _logger;

        public DomainDetectionService(
            IOptions<ConfigurationSettings> configurationSettings,
            ILogger<DomainDetectionService> logger,
            IRequestClient<DomainDetectionRequest> requestClient
        )
        {
            _configurationSettings = configurationSettings.Value;
            _logger = logger;
            _requestClient = requestClient;
        }

        public async Task<DomainDetectionResponse> Detect(DomainDetectionRequest detectionRequest)
        {
            using var request = _requestClient.Create(detectionRequest, timeout: _configurationSettings.DomainDetectionTimeout);
            request.UseExecute(x => x.AddRequestHeaders<DomainDetectionResponse>());
            var translationResponse = await request.GetResponse<DomainDetectionResponse>();

            return translationResponse.Message;
        }
    }
}
