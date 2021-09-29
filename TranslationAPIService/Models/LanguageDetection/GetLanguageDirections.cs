using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models.LanguageDetection
{
    public class GetLanguageDirections: APIResponse
    {
        public IEnumerable<LanguageDirection> LanguageDirections { get; set; }
    }
}
