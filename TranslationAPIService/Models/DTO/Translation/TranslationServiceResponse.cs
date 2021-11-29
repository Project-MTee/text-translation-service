using System.Collections.Generic;

namespace Tilde.MT.TranslationAPIService.Models.DTO.Translation
{
    public record TranslationServiceResponse
    {
        /// <summary>
        /// Translation result
        /// </summary>
        public IEnumerable<string> Translations { get; init; }
    }
}
