using System;

namespace Tilde.MT.TranslationAPIService.Exceptions.DomainDetection
{
    public class DomainDetectionTimeoutException : Exception
    {
        public DomainDetectionTimeoutException() : base("Domain detection timeout")
        {

        }
    }
}
