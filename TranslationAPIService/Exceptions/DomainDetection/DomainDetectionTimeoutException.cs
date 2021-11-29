using System;

namespace Tilde.MT.TranslationAPIService.Exceptions.DomainDetection
{
    public class DomainDetectionTimeoutException : Exception
    {
        public DomainDetectionTimeoutException(TimeSpan timeout):
            base($"Domain detection timed out in: {timeout}")
        {

        }
    }
}
