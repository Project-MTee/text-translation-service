using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Tilde.MT.TranslationAPIService.Extensions
{
    public static class CorsExtensions
    {
        private static readonly string DevelopmentCorsPolicy = "development-policy";

        public static IServiceCollection AddCorsPolicies(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(
                    name: DevelopmentCorsPolicy,
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:4200").AllowAnyHeader();
                    }
                );
            });

            return services;
        }

        public static IApplicationBuilder UseCorsPolicies(this IApplicationBuilder app)
        {
            app.UseCors(DevelopmentCorsPolicy);

            return app;
        }
    }
}
