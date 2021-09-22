using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tilde.MT.TranslationAPIService.Models;

namespace Tilde.MT.TranslationAPIService.Controllers
{
    /// <summary>
    /// API for language directions
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class LanguageDirectionController : ControllerBase
    {
        private readonly ILogger<LanguageDirectionController> _logger;

        public LanguageDirectionController(ILogger<LanguageDirectionController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get available language directions
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<LanguageDirection> Get()
        {
            return Ok();
        }
    }
}
