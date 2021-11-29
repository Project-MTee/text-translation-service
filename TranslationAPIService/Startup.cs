using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tilde.MT.TranslationAPIService.Extensions;
using Tilde.MT.TranslationAPIService.Models.Configuration;
using Tilde.MT.TranslationAPIService.Models.Mappings;
using Tilde.MT.TranslationAPIService.Services;

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
            services.AddHttpClient();
            services.AddMemoryCache();

            services.AddCorsPolicies();

            services.Configure<ConfigurationServices>(Configuration.GetSection("Services"));
            services.Configure<ConfigurationSettings>(Configuration.GetSection("Configuration"));

            services.AddControllers();

            services.AddDocumentation();

            var mappingConfig = new MapperConfiguration(config =>
            {
                config.AddProfile(new MappingProfile());
            });
            services.AddSingleton(mappingConfig.CreateMapper());

            services.AddMessaging(Configuration);

            services.AddScoped<IDomainDetectionService, DomainDetectionService>();
            services.AddScoped<ITranslationService, TranslationService>();
            services.AddSingleton<ILanguageDirectionService, LanguageDirectionService>();

            services.AddClientErrorProcessing();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
#if DEBUG
            app.UseDocumentation();
#endif

            app.UseUnhandledExceptionProcessing();

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
#if DEBUG
            app.UseCorsPolicies();
#endif
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // Startup probe / readyness probe
                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions()
                {
                    // check if MassTransit can connect 
                    Predicate = (check) =>
                    {
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
