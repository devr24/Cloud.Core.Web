namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using AspNetCore.Builder;
    using AspNetCore.Mvc;
    using AspNetCore.Mvc.Abstractions;
    using OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using Swashbuckle.AspNetCore.SwaggerUI;

    /// <summary>
    /// Class ServiceCollectionExtensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Use the swagger UI with versions documented.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="versions">The versions.</param>
        /// <param name="routePrepend">Segment to prepend the Uri with.</param>
        /// <param name="additionalConfig">UI configuration action.</param>
        /// <returns>IApplicationBuilder.</returns>
        public static IApplicationBuilder UseSwaggerWithVersion(this IApplicationBuilder app, double[] versions, string routePrepend = null, Action<SwaggerUIOptions> additionalConfig = null)
        {
            // Use swagger in the application.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {

                // Run additional config method if set.
                additionalConfig?.Invoke(c);

                foreach (var version in versions)
                {
                    routePrepend = routePrepend.IsNullOrEmpty() ? string.Empty : $"/{routePrepend}";
                    c.SwaggerEndpoint($"{routePrepend}/swagger/v{version:F1}/swagger.json", $"{Assembly.GetEntryAssembly().GetAssemblyName()} Api {version:F1}");
                }
            });

            return app;
        }

        /// <summary>
        /// Adds the swagger functionality with api versioning - will document each version passed.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="versions">The versions.</param>
        /// <param name="additionalConfig">The additional configuration.</param>
        /// <returns>IServiceCollection.</returns>
        [ExcludeFromCodeCoverage] // Need to complete the testing from this.
        public static IServiceCollection AddSwaggerWithVersions(this IServiceCollection services, double[] versions, Action<SwaggerGenOptions> additionalConfig = null)
        {
            var latestVersion = versions.Max();
            var versionString = latestVersion.ToString(CultureInfo.InvariantCulture).Split('.');

            var major = Convert.ToInt16(versionString[0]);
            var minor = versionString.Length == 1 ? 0 : Convert.ToInt16(versionString[1]);

            // Add Api Version strategy.
            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(major, minor);
            });

            services.AddSwaggerGen(c =>
            {
                // Add the user defined settings first.
                additionalConfig?.Invoke(c);

                c.EnableAnnotations();

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "API Bearer Token Authentication",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme()
                        {
                            Reference = new OpenApiReference
                            {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        }, new List<string>()
                    }
                });

                foreach (var version in versions)
                {
                    var versionNumber = $"v{version:F1}";
                    c.SwaggerDoc(versionNumber, new OpenApiInfo { Title = $"{Assembly.GetEntryAssembly().GetAssemblyName()} {version:F1}", Version = versionNumber });
                }

                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var actionApiVersionModel = apiDesc.ActionDescriptor?.GetApiVersion();
                    // would mean this action is unversioned and should be included everywhere
                    if (actionApiVersionModel == null)
                    {
                        return true;
                    }
                    if (actionApiVersionModel.DeclaredApiVersions.Any())
                    {
                        return actionApiVersionModel.DeclaredApiVersions.Any(v => $"v{v.ToString()}" == docName);
                    }
                    return actionApiVersionModel.ImplementedApiVersions.Any(v => $"v{v.ToString()}" == docName);
                });
            });

            return services;
        }
    }
}
