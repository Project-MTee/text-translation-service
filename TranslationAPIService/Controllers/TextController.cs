using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tilde.MT.TranslationAPIService.Models;

namespace Tilde.MT.TranslationAPIService.TranslationAPI.Controllers
{
    /// <summary>
    /// API for text translation
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class TextController : ControllerBase
    {
        private readonly ILogger<TextController> _logger;

        public TextController(ILogger<TextController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Translate text
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult<Translation> GetTranslation(TranslationRequest request)
        {
            return Ok();
        }
    }
}
