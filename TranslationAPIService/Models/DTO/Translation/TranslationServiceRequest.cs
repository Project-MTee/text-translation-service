using System.Collections.Generic;

namespace Tilde.MT.TranslationAPIService.Models.DTO.Translation
{
    public class TranslationServiceRequest
    {
        public List<string> Text { get; set; }
        public string SourceLanguage { get; set; }
        public string TargetLanguage { get; set; }
        public string Domain { get; set; }
        public string InputType { get; set; }
    }
}
