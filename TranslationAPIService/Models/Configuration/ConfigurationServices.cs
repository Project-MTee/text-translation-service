namespace Tilde.MT.TranslationAPIService.Models.Configuration
{
    public record ConfigurationServices
    {
        public Services.RabbitMQ RabbitMQ { get; init; }
        public Services.TranslationSystem TranslationSystem { get; init; }
    }
}
