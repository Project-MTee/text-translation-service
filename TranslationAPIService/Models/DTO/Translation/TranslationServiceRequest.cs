using System.Collections.Generic;

namespace Tilde.MT.TranslationAPIService.Models.DTO.Translation
{
    public record TranslationServiceRequest
    {
        public List<string> Text { get; init; }
        public string SourceLanguage { get; init; }
        public string TargetLanguage { get; init; }
        public string Domain { get; init; }
        public string InputType { get; init; }
    }
}
