namespace Microsoft.AspNetCore.Mvc.Abstractions
{
    using System;
    using System.Linq;
    using Versioning;

    /// <summary>
    /// Versioning ActionDescriptor Extensions.
    /// </summary>
    public static class ActionDescriptorExtensions
    {
        /// <summary>
        /// Gets the API version.
        /// </summary>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <returns>ApiVersionModel.</returns>
        public static ApiVersionModel GetApiVersion(this ActionDescriptor actionDescriptor)
        {
            return actionDescriptor?.Properties
                .Where((kvp) => (Type)kvp.Key == typeof(ApiVersionModel))
              .Select(kvp => kvp.Value as ApiVersionModel).FirstOrDefault();
        }
    }
}
