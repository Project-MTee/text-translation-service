using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Services
{
    public interface IDomainDetectionService
    {
        Task<string> Detect(string sourceLanguage, List<string> text);
    }
}
