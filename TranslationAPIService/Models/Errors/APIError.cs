using System.Text.Json.Serialization;

namespace Tilde.MT.TranslationAPIService.Models.Errors
{
    public record APIError
    {
        [JsonPropertyName("error")]
        public Error Error { get; init; }
    }
}
