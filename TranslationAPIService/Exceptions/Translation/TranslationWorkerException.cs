using System;

namespace Tilde.MT.TranslationAPIService.Exceptions.Translation
{
    public class TranslationWorkerException : Exception
    {
        public int StatusCode { get; }
        public string StatusMessage { get; }

        public TranslationWorkerException(int statusCode, string statusMessage) :
            base($"Translation worker failed to process translation with status code '{statusCode}' and status message: '{statusMessage}'")
        {
            StatusCode = statusCode;
            StatusMessage = statusMessage;
        }
    }
}
