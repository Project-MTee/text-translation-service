using AutoMapper;

namespace Tilde.MT.TranslationAPIService.Models.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region RabbitMQ mappings

            CreateMap<Models.DTO.Translation.TranslationRequest, Models.RabbitMQ.Translation.TranslationRequest>();
            CreateMap<Models.RabbitMQ.Translation.TranslationResponse, Models.DTO.Translation.Translation>();

            CreateMap<Models.DTO.Translation.TranslationServiceRequest, Models.RabbitMQ.Translation.TranslationRequest>();
            CreateMap<Models.RabbitMQ.Translation.TranslationResponse, Models.DTO.Translation.TranslationServiceResponse>();

            #endregion

            CreateMap<Models.DTO.Translation.TranslationRequest, Models.DTO.Translation.TranslationServiceRequest>();
        }
    }
}
