namespace Tilde.MT.TranslationAPIService.Models.DTO.Translation
{
    public record TranslationServiceResponse
    {
        /// <summary>
        /// Translation result
        /// </summary>
        public string[] Translations { get; init; }
    }
}
