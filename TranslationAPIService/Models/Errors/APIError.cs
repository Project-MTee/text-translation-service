using System.Text.Json.Serialization;

namespace Tilde.MT.TranslationAPIService.Models.Errors
{
    public class APIError
    {
        [JsonPropertyName("error")]
        public Error Error { get; set; }
    }
}
