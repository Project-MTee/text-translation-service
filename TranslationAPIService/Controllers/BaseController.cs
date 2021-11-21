using Microsoft.AspNetCore.Mvc;
using System.Net;
using Tilde.MT.TranslationAPIService.Enums;
using Tilde.MT.TranslationAPIService.Extensions;
using Tilde.MT.TranslationAPIService.Models.Errors;

namespace Tilde.MT.TranslationAPIService.Controllers
{
    public abstract class BaseController: Controller
    {
        protected ObjectResult FormatAPIError(HttpStatusCode status, ErrorSubCode subcode, HttpStatusCode? messageStatusCode = null, string message = null)
        {
            return StatusCode(
                (int)status,
                new APIError()
                {
                    Error = new Error()
                    {
                        Code = (int)(messageStatusCode ?? status) * 1000 + (int)subcode,
                        Message = message ?? subcode.Description()
                    }
                }
            );
        }
    }
}
