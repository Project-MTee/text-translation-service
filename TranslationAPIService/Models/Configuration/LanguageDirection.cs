using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models.Configuration
{
    public class LanguageDirection
    {
        public string SourceLanguage { get; set; }
        public string TargetLanguage { get; set; }
        public string Domain { get; set; }
    }
}
