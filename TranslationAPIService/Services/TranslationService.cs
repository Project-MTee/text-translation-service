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
        private readonly IRequestClient<TranslationRequest> _requestClient;

        public TranslationService(
            IOptions<ConfigurationSettings> configurationSettings,
            IRequestClient<TranslationRequest> requestClient
        )
        {
            _configurationSettings = configurationSettings.Value;
            _requestClient = requestClient;
        }

        public async Task<TranslationResponse> Translate(TranslationRequest translationRequest)
        {
            using var request = _requestClient.Create(translationRequest, timeout: _configurationSettings.TranslationTimeout);
            request.UseExecute(x => x.AddRequestHeaders<TranslationResponse>());
            var translationResponse = await request.GetResponse<TranslationResponse>();

            return translationResponse.Message;
        }
    }
}
