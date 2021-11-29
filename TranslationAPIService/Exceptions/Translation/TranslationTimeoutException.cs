using System;

namespace Tilde.MT.TranslationAPIService.Exceptions.Translation
{
    public class TranslationTimeoutException: Exception
    {
        public TranslationTimeoutException(TimeSpan timeout) : 
            base($"Translation request timed out in: {timeout}")
        {

        }
    }
}
