using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tilde.MT.TranslationAPIService.Models;
using Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation;
using Tilde.MT.TranslationAPIService.Models.Translation;
using Tilde.MT.TranslationAPIService.Services;

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
        private readonly IMapper _mapper;
        private readonly TranslationService _translationService;
        

        public TextController(
            ILogger<TextController> logger, 
            IMapper mapper,
            TranslationService translationService
        )
        {
            _logger = logger;
            _mapper = mapper;
            _translationService = translationService;
        }

        /// <summary>
        /// Translate text
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Translation>> GetTranslation(Models.Translation.RequestTranslation request)
        {
            var response = await _translationService.Translate(request);
            return Ok(response);
        }
    }
}
