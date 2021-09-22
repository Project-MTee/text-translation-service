using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models
{
    public class LanguageDirection
    {
        public string SrcLang { get; set; }
        public string TrgLang { get; set; }
        public string Domain { get; set; }
    }
}
