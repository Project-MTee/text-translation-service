using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using MassTransit;
using Tilde.MT.TranslationAPIService.Models.Configuration;
using AutoMapper;
using Tilde.MT.TranslationAPIService.Models.Mappings;
using Tilde.MT.TranslationAPIService.Services;
using RabbitMQ.Client;

namespace Tilde.MT.TranslationAPIService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        readonly string DevelopmentCorsPolicy = "development-policy";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCors(options =>
            {
                options.AddPolicy(name: DevelopmentCorsPolicy,
                                  builder =>
                                  {
                                      builder.WithOrigins("http://localhost:4200").AllowAnyHeader();
                                  });
            });
            var serviceConfiguration = Configuration.GetSection("Services").Get<ConfigurationServices>();
            var configurationSettings = Configuration.GetSection("Configuration").Get<ConfigurationSettings>();
            services.Configure<ConfigurationServices>(Configuration.GetSection("Services"));
            services.Configure<ConfigurationSettings>(Configuration.GetSection("Configuration"));

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TranslationAPI", Version = "v1" });
                c.EnableAnnotations();
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{nameof(Tilde)}.{nameof(Tilde.MT)}.{nameof(Tilde.MT.TranslationAPIService)}.xml"));
            });

            var mappingConfig = new MapperConfiguration(config =>
            {
                config.AddProfile(new MappingProfile());
            });
            services.AddSingleton(mappingConfig.CreateMapper());

            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();

                x.AddRequestClient<Models.RabbitMQ.Translation.TranslationRequest>();
                x.AddRequestClient<Models.RabbitMQ.DomainDetection.DomainDetectionRequest>();

                x.UsingRabbitMq((context, config) =>
                {
                    config.Host(serviceConfiguration.RabbitMQ.Host, "/", host =>
                    {
                        host.Username(serviceConfiguration.RabbitMQ.UserName);
                        host.Password(serviceConfiguration.RabbitMQ.Password);
                    });

                    #region Translation configuration

                    config.Message<Models.RabbitMQ.Translation.TranslationRequest>(x =>
                    {
                        x.SetEntityName("translation");
                    });
                    config.Publish<Models.RabbitMQ.Translation.TranslationRequest>(x =>
                    {
                        x.ExchangeType = ExchangeType.Direct;
                        x.Durable = false;
                    });
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

                    config.Message<Models.RabbitMQ.DomainDetection.DomainDetectionRequest>(x =>
                    {
                        x.SetEntityName("domain-detection");
                    });
                    config.Publish<Models.RabbitMQ.Translation.TranslationRequest>(x =>
                    {
                        x.ExchangeType = ExchangeType.Direct;
                        x.Durable = false;
                    });
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
                        MassTransit.Serialization.RawJsonSerializerOptions.AddTransportHeaders
                    );
                });
            });

            services.AddMassTransitHostedService(false);

            services.AddScoped<DomainDetectionService>();
            services.AddScoped<TranslationService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TranslationAPI v1"));
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseCors(DevelopmentCorsPolicy);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
