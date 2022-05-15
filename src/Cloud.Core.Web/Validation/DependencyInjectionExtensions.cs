using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Cloud.Core.Web.Validation
{
    /// <summary>Dependency injection extensions class.</summary>
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Add translation validation services.
        /// </summary>
        /// <param name="builder">IMvcBuilder to extend.</param>
        /// <returns>IMvcBuilder extended.</returns>
        public static IMvcBuilder AddTranslatableValidation(this IMvcBuilder builder)
        {
            return builder.ConfigureApiBehaviorOptions(setupAction =>
            {
                setupAction.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState); ;

                    problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);

                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });
        }
    }
}
