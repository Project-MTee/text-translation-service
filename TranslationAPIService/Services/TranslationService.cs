using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Tilde.MT.TranslationAPIService.Extensions.MassTransit;
using Tilde.MT.TranslationAPIService.Models.Configuration;
using Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation;

namespace Tilde.MT.TranslationAPIService.Services
{
    public class TranslationService
    {
        private readonly ConfigurationSettings _configurationSettings;
        private readonly IRequestClient<TranslationRequest> _client;
        private readonly ILogger _logger;

        public TranslationService(
            IOptions<ConfigurationSettings> configurationSettings,
            ILogger<DomainDetectionService> logger,
            IBus bus
        )
        {
            _configurationSettings = configurationSettings.Value;
            _logger = logger;

            var addr = new Uri($"exchange:translation?type=direct&durable=false");
            _client = bus.CreateRequestClient<TranslationRequest>(addr);
        }

        public async Task<TranslationResponse> Translate(TranslationRequest translationRequest)
        {
            using var request = _client.Create(translationRequest, timeout: _configurationSettings.TranslationTimeout);
            request.UseExecute(x => x.AddRequestHeaders<TranslationResponse>());
            var translationResponse = await request.GetResponse<TranslationResponse>();

            return translationResponse.Message;
        }
    }
}
