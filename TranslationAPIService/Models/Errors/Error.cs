
using System.Text.Json.Serialization;

namespace Tilde.MT.TranslationAPIService.Models.Errors
{
    public record Error
    {
        /// <summary>
        /// Error code consisting of HTTP status code and error ID
        /// </summary>
        /// <example>500001</example>
        [JsonPropertyName("code")]
        public int Code { get; init; }

        /// <summary>
        /// Textual message of error
        /// </summary>
        /// <example>Error message</example>
        [JsonPropertyName("message")]
        public string Message { get; init; }
    }
}
