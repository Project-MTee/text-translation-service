using System;

namespace Tilde.MT.TranslationAPIService.Exceptions.LanguageDirection
{
    public class LanguageDirectionNotFoundException : Exception
    {
        public LanguageDirectionNotFoundException(string domain, string sourceLanguage, string targetLanguage) :
            base($"Translation direction with domain: '{domain}', source language: '{sourceLanguage}', target language: '{targetLanguage}' is not found")
        {

        }
    }
}
