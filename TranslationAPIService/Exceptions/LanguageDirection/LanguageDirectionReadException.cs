using System;

namespace Tilde.MT.TranslationAPIService.Exceptions.LanguageDirection
{
    public class LanguageDirectionReadException : Exception
    {
        public LanguageDirectionReadException() : base("Language directions cannot be read")
        {

        }
    }
}
