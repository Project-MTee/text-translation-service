using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Tilde.MT.TranslationAPIService.Models.DTO.Translation;
using Tilde.MT.TranslationAPIService.Models.RabbitMQ.Translation;

namespace Tilde.MT.TranslationAPIService.Models.Mappings
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            #region RabbitMQ mappings

            CreateMap<RequestTranslation, Models.RabbitMQ.Translation.TranslationRequest>();
            CreateMap<Models.RabbitMQ.Translation.TranslationResponse, DTO.Translation.Translation>();

            #endregion
        }
    }
}
