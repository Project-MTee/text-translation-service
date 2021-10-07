
using System.Text.Json.Serialization;

namespace Tilde.MT.TranslationAPIService.Models
{
    public class Error
    {
        /// <summary>
        /// Error code
        /// </summary>
        /// <example>500</example>
        [JsonPropertyName("code")]
        public int Code { get; set; }

        /// <summary>
        /// Textual message of error
        /// </summary>
        /// <example>Error message</example>
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
