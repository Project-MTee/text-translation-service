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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var serviceConfiguration = Configuration.GetSection("Services").Get<ConfigurationServices>();
            services.Configure<ConfigurationSettings>(Configuration.GetSection("Configuration"));

            services.AddMassTransit(x =>
            {
                x.AddRequestClient<Models.RabbitMQ.Translation.TranslationRequest>();
                x.AddRequestClient<Models.RabbitMQ.DomainDetection.DomainDetectionRequest>();
                /*x.AddRequestClient<TranslateRequest>();

                x.SetKebabCaseEndpointNameFormatter();
                */

                x.UsingRabbitMq((context, config) =>
                {
                    config.Host(serviceConfiguration.RabbitMQ.Host, "/", host =>
                    {
                        host.Username(serviceConfiguration.RabbitMQ.UserName);
                        host.Password(serviceConfiguration.RabbitMQ.Password);
                    });

                    config.Publish<Models.RabbitMQ.DomainDetection.DomainDetectionRequest>(x =>
                    {
                        x.ExchangeType = ExchangeType.Direct;
                        x.BindQueue("foo.bar", "foo.bar", qc =>
                        {
                            qc.ExchangeType = ExchangeType.Direct;
                        });
                    });
                });
            });

            services.AddMassTransitHostedService();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TranslationAPI", Version = "v1" });
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{nameof(Tilde)}.{nameof(Tilde.MT)}.{nameof(Tilde.MT.TranslationAPIService)}.xml"));
            });

            var mappingConfig = new MapperConfiguration(config =>
            {
                config.AddProfile(new MappingProfile());
            });
            services.AddSingleton(mappingConfig.CreateMapper());
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

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
