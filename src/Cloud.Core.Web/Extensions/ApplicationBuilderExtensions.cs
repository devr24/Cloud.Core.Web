namespace Microsoft.AspNetCore.Builder
{
    using Http;
    using Cloud.Core.Web;

    /// <summary>
    /// Extension methods for IApplicationBuilder
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Uses the unhandled exception middleware to capture any exceptions, log them and then output a meaning response of ApiErrorResponse.
        /// </summary>
        /// <param name="builder">The builder to extend.</param>
        /// <returns>The builder with the additional middleware attached.</returns>
        public static IApplicationBuilder UseUnhandledExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UnhandledExceptionMiddleware>();
        }

        /// <summary>
        /// Adds the health probe.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="endpointRoute">The endpoint route.</param>
        /// <returns>The builder with the additional probe endpoint attached.</returns>
        public static IApplicationBuilder AddHealthProbe(this IApplicationBuilder app, string endpointRoute = "probe")
        {
            app.Map($"/{endpointRoute}", mapRun =>
            {
                mapRun.Run(async context =>
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("Running");
                });
            });

            return app;
        }
    }
}
