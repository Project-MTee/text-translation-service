using System;

namespace Tilde.MT.TranslationAPIService.Exceptions.LanguageDirection
{
    public class LanguageDirectionReadException : Exception
    {
        public LanguageDirectionReadException() : base("Lanuage directions cannot be read")
        {

        }
    }
}
