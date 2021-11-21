using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using Tilde.MT.TranslationAPIService.Models.Configuration;
using MassTransit.Serialization;

namespace Tilde.MT.TranslationAPIService.Extensions
{
    public static class MessagingExtensions
    {
        public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();

                x.AddRequestClient<Models.RabbitMQ.Translation.TranslationRequest>();
                x.AddRequestClient<Models.RabbitMQ.DomainDetection.DomainDetectionRequest>();

                x.UsingRabbitMq((context, config) =>
                {
                    var serviceConfiguration = configuration.GetSection("Services").Get<ConfigurationServices>();

                    config.Host(serviceConfiguration.RabbitMQ.Host, "/", host =>
                    {
                        host.Username(serviceConfiguration.RabbitMQ.UserName);
                        host.Password(serviceConfiguration.RabbitMQ.Password);
                    });

                    #region Translation configuration

                    // Specify exchange
                    config.Message<Models.RabbitMQ.Translation.TranslationRequest>(x =>
                    {
                        x.SetEntityName("translation");
                    });

                    // Set exchange options
                    config.Publish<Models.RabbitMQ.Translation.TranslationRequest>(x =>
                    {
                        x.ExchangeType = ExchangeType.Direct;
                        x.Durable = false;
                    });

                    // Set message attributes
                    config.Send<Models.RabbitMQ.Translation.TranslationRequest>(x =>
                    {
                        x.UseRoutingKeyFormatter(context =>
                        {
                            return $"translation.{context.Message.SourceLanguage}.{context.Message.TargetLanguage}.{context.Message.Domain}.{context.Message.InputType}";
                        });

                        x.UseCorrelationId(context => Guid.NewGuid());
                    });

                    #endregion

                    #region Domain detection configuration

                    // Specify exchange
                    config.Message<Models.RabbitMQ.DomainDetection.DomainDetectionRequest>(x =>
                    {
                        x.SetEntityName("domain-detection");
                    });

                    // Set exchange options
                    config.Publish<Models.RabbitMQ.DomainDetection.DomainDetectionRequest>(x =>
                    {
                        x.ExchangeType = ExchangeType.Direct;
                        x.Durable = false;
                    });

                    // Set message attributes
                    config.Send<Models.RabbitMQ.DomainDetection.DomainDetectionRequest>(x =>
                    {
                        x.UseRoutingKeyFormatter(context =>
                        {
                            return $"domain-detection.{context.Message.SourceLanguage}";
                        });

                        x.UseCorrelationId(context => Guid.NewGuid());
                    });

                    #endregion

                    config.ConfigureEndpoints(context);

                    config.UseRawJsonSerializer(
                        RawJsonSerializerOptions.AddTransportHeaders
                    );
                });
            });

            services.AddMassTransitHostedService(false);

            return services;
        }
    }
}
