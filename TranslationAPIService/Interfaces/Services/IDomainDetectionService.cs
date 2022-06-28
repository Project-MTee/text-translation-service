using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Interfaces.Services
{
    public interface IDomainDetectionService
    {
        /// <returns></returns>
        /// <summary>
        /// Detect domain using domain detection worker
        /// </summary>
        /// <param name="sourceLanguage"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        Task<string> Detect(string sourceLanguage, List<string> text);
    }
}
