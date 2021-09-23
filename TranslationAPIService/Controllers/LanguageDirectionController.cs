using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using Tilde.MT.TranslationAPIService.Models.Configuration;

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
        private readonly ConfigurationSettings _configurationSettings;
        private readonly IMapper _mapper;

        public LanguageDirectionController(
            ILogger<LanguageDirectionController> logger,
            IOptions<ConfigurationSettings> configurationSettings,
            IMapper mapper
        )
        {
            _logger = logger;
            _configurationSettings = configurationSettings.Value;
            _mapper = mapper;
        }

        /// <summary>
        /// Get available language directions
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<IEnumerable<Models.LanguageDirection>> Get()
        {
            var languageDirections = _configurationSettings.LanguageDirections.Select(item => _mapper.Map<Models.LanguageDirection>(item));
            return Ok(languageDirections);
        }
    }
}
