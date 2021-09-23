using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation;

namespace Tilde.MT.TranslationAPIService.Models.Mappings
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            #region RabbitMQ mappings

            CreateMap<Models.TranslationRequest, RabbitMQ.Translation.TranslationRequest>();
            CreateMap<RabbitMQ.Translation.TranslationResponse, Models.Translation>();

            #endregion

            CreateMap<Models.Configuration.LanguageDirection, Models.LanguageDirection>();
        }
    }
}
