using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Services
{
    public interface ILanguageDirectionService
    {
        Task Validate(string domain, string sourceLanguage, string targetLanguage);
    }
}
