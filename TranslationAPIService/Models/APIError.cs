using System.Text.Json.Serialization;

namespace Tilde.MT.TranslationAPIService.Models
{
    public class APIError
    {
        [JsonPropertyName("error")]
        public Error Error { get; set; }
    }
}
