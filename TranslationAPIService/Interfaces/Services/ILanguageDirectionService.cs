using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Interfaces.Services
{
    public interface ILanguageDirectionService
    {
        Task Validate(string domain, string sourceLanguage, string targetLanguage);
    }
}
