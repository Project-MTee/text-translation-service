using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Linq;
using System.Net;
using System.Text.Json;
using Tilde.MT.TranslationAPIService.Models.Errors;

namespace Tilde.MT.TranslationAPIService.Extensions
{
    public static class ErrorHandingExtensions
    {
        /// <summary>
        /// Catch and format client error exceptions
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddClientErrorProcessing(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
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

            return services;
        }

        /// <summary>
        /// Catch and format unhandled exceptions
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseUnhandledExceptionProcessing(this IApplicationBuilder app)
        {
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

            return app;
        }
    }
}
