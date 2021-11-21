namespace Tilde.MT.TranslationAPIService.Models.Configuration.Services
{
    public record RabbitMQ
    {
        public string Host { get; init; }
        public string UserName { get; init; }
        public string Password { get; init; }
        public int Port { get; init; } = 5672;
    }
}
