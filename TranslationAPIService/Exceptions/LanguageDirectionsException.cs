using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Exceptions
{
    public class LanguageDirectionsException: Exception
    {
        public LanguageDirectionsException(string message) : base($"Language Direction exception: {message}")
        {

        }
    }
}
