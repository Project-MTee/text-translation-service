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
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Serilog;
using System.Linq;
using Tilde.MT.TranslationAPIService.Extensions;
using Tilde.MT.TranslationAPIService.Models.Errors;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

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
            services.AddHttpClient();
            services.AddMemoryCache();

            services.AddCors(options =>
            {
                options.AddPolicy(name: DevelopmentCorsPolicy,
                                  builder =>
                                  {
                                      builder.WithOrigins("http://localhost:4200").AllowAnyHeader();
                                  });
            });
            
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
                    var serviceConfiguration = Configuration.GetSection("Services").Get<ConfigurationServices>();

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
                        MassTransit.Serialization.RawJsonSerializerOptions.AddTransportHeaders
                    );
                });
            });

            services.AddMassTransitHostedService(false);

            services.AddScoped<DomainDetectionService>();
            services.AddScoped<TranslationService>();
            services.AddSingleton<LanguageDirectionService>();

            // Catch client errors
            services.Configure<ApiBehaviorOptions>(options => {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var modelStateEntries = actionContext.ModelState.Where(e => e.Value.Errors.Count > 0).ToArray();
                    var requestTooLarge = modelStateEntries.Where(item =>
                    {
                        return item.Value.Errors.Where(err => err.ErrorMessage.Contains("Request body too large")).Any();
                    }).Any();

                    if (requestTooLarge)
                    {
                        actionContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.RequestEntityTooLarge;

                        return new JsonResult(
                            new APIError()
                            {
                                Error = new Error()
                                {
                                    Code = ((int)HttpStatusCode.RequestEntityTooLarge) * 1000 + (int)Enums.ErrorSubCode.GatewayRequestTooLarge,
                                    Message = Enums.ErrorSubCode.GatewayRequestTooLarge.Description()
                                }
                            }
                        );
                    }
                    else
                    {
                        return new BadRequestObjectResult(
                            new APIError()
                            {
                                Error = new Error()
                                {
                                    Code = ((int)HttpStatusCode.BadRequest) * 1000 + (int)Enums.ErrorSubCode.GatewayRequestValidation,
                                    Message = Enums.ErrorSubCode.GatewayRequestValidation.Description()
                                }
                            }
                        );
                    }
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
#if DEBUG
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TranslationAPI v1"));
#endif

            // Catch all unexpected errors
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {

                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        string response;
                        if (contextFeature.Error.Message.Contains("Request body too large"))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.RequestEntityTooLarge;
                            Log.Error($"Request too large: {contextFeature.Error}");

                            response = JsonSerializer.Serialize(new APIError()
                            {
                                Error = new Error()
                                {
                                    Code = ((int)HttpStatusCode.RequestEntityTooLarge) * 1000 + (int)Enums.ErrorSubCode.GatewayRequestTooLarge,
                                    Message = Enums.ErrorSubCode.GatewayRequestTooLarge.Description()
                                }
                            });
                        }
                        else
                        {
                            Log.Error($"Unexpected error: {contextFeature.Error}");
                            response = JsonSerializer.Serialize(new APIError()
                            {
                                Error = new Error()
                                {
                                    Code = ((int)HttpStatusCode.InternalServerError) * 1000 + (int)Enums.ErrorSubCode.GatewayGeneric,
                                    Message = Enums.ErrorSubCode.GatewayGeneric.Description()
                                }
                            });
                        }

                        await context.Response.WriteAsync(response);
                    }
                });
            });

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
#if DEBUG
            app.UseCors(DevelopmentCorsPolicy);
#endif
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // Startup probe / readyness probe
                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions()
                {
                    // check if MassTransit can connect 
                    Predicate = (check) => {
                        return check.Tags.Contains("ready");
                    }
                });

                // Liveness 
                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions()
                {

                });
            });
        }
    }
}
