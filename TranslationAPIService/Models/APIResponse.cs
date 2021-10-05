using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models
{
    public interface APIResponse
    {
        public Error Error { get; set; }
    }
}
