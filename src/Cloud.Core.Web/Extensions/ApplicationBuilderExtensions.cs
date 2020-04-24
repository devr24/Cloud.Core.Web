namespace Microsoft.AspNetCore.Builder
{
    using Http;
    using Cloud.Core.Web.Middleware;
    using System.Collections.Generic;
    using Localization;
    using System.Globalization;
    using System;
    using System.Linq;

    /// <summary>
    /// Extension methods for IApplicationBuilder.
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
        /// Uses the localization.
        /// </summary>
        /// <param name="builder">The builder to extend.</param>
        /// <param name="supportedCultures">The supported cultures string list.</param>
        /// <param name="defaultCulture">The default culture when none has been set.</param>
        /// <param name="testLanguageCultureCode">The test language code (used when transforming from TL to this culture as it requires a real culture).</param>
        /// <returns>IApplicationBuilder.</returns>
        /// <exception cref="ArgumentException">Default culture of \"{defaultCulture}\" is not specified in supported cultures list</exception>
        public static IApplicationBuilder UseLocalization(this IApplicationBuilder builder, string[] supportedCultures, string defaultCulture = "en", string testLanguageCultureCode = "ts")
        {
            // Make sure the default culture is defined!
            if (supportedCultures.Contains(defaultCulture) == false)
            {
                throw new ArgumentException($"Default culture of \"{defaultCulture}\" is not specified in supported cultures list");
            }

            // Build a useable list of cultures from the culture array.
            bool hasTestLang = false;
            var cultures = new List<CultureInfo>();
            foreach (var culture in supportedCultures)
            {
                if (culture == "ts")
                {
                    hasTestLang = true;
                }

                cultures.Add(new CultureInfo(culture));
            }

            if (!hasTestLang)
            {
                cultures.Add(new CultureInfo(testLanguageCultureCode));
            }

            // Middleware to guarantee correct culture info is set when TL (test language) is sent through.
            builder.Use(async (context, next) =>
            {
                var culture = context.Request.Query["culture"];

                if (culture.IsNullOrDefault())
                {
                    culture = context.Request.Headers["Accept-Language"];
                }

                if (culture == "tl")
                {
                    context.Request.Headers["Accept-Language"] = testLanguageCultureCode;
                }

                // Call the next delegate/middleware in the pipeline
                await next();
            });

            // Add localisation/translations by using culture code.
            builder.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(defaultCulture),
                SupportedCultures = cultures,
                SupportedUICultures = cultures
            });

            return builder;
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
