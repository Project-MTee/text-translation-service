using System.Threading.Tasks;
using Tilde.MT.TranslationAPIService.Models.DTO.Translation;

namespace Tilde.MT.TranslationAPIService.Interfaces.Services
{
    public interface ITranslationService
    {
        Task<TranslationServiceResponse> Translate(TranslationServiceRequest translationRequest);
    }
}
