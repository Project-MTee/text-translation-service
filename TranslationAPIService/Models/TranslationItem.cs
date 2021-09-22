using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models
{
    /// <summary>
    /// Translated information for one translation
    /// </summary>
    public class TranslationItem
    {
        /// <summary>
        /// Translated text
        /// </summary>
        public string Translation { get; set; }
    }
}
