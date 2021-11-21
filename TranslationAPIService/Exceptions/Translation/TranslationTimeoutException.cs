using System;

namespace Tilde.MT.TranslationAPIService.Exceptions.Translation
{
    public class TranslationTimeoutException : Exception
    {
        public TranslationTimeoutException() : base("Translation request timed out")
        {

        }
    }
}
