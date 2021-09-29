using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models
{
    public class ServiceResponse<T>
    {
        public T Data { get; set; }
        public Error Error { get; set; }
    }
}
