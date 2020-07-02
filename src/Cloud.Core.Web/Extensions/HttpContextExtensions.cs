namespace Microsoft.AspNetCore.Http
{
    using System;
    using System.Linq;

    /// <summary>
    /// Extensions to HttpContext class.
    /// </summary>
    public static class HttpContextExtension
    {
        /// <summary>Gets the request locale.</summary>
        /// <param name="context">The context.</param>
        /// <param name="defaultCulture">The default culture to use if not set in the request.</param>
        /// <returns>ProviderCultureResult.</returns>
        public static string GetRequestLocale(this HttpContext context, string defaultCulture = "en")
        {
            var userLangs = context.Request.Headers["Accept-Language"].ToString();
            var firstLang = userLangs.Split(',').FirstOrDefault();
            var defaultLang = string.IsNullOrEmpty(firstLang) ? defaultCulture : firstLang;
            return defaultLang;
        }

        /// <summary>
        /// Gets the claim value corresponding to the passed in key.
        /// </summary>
        /// <typeparam name="T">Type of object expected to be found.</typeparam>
        /// <param name="context">The context to find the claim value from.</param>
        /// <param name="key">The key to search claims for.</param>
        /// <returns>Found claim value of type T.</returns>
        public static T GetClaimValue<T>(this HttpContext context, string key)
        {
            // Having found a key string to be found, loop through the claims until it is identified.
            foreach (var claim in context.User.Claims)
            {
                // If the claim.type matches the searched key, then get the claims value.
                if (string.CompareOrdinal(claim.Type, key) == 0)
                    return (T)Convert.ChangeType(claim.Value, typeof(T));
            }

            return default;
        }

        /// <summary>
        /// Gets the value from the HttpContext request headers.
        /// </summary>
        /// <param name="context">The context to find the header value from.</param>
        /// <param name="key">The key of which header to find.</param>
        /// <returns>String value from the header.</returns>
        public static string GetRequestHeader(this HttpContext context, string key)
        {
            if (context.Request.Headers.TryGetValue(key, out var header))
                return header[0];
            
            // Default.
            return null;
        }

        /// <summary>
        /// Gets the client ip address.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>String client IP.</returns>
        public static string GetClientIpAddress(this HttpContext context) => context.Connection.RemoteIpAddress?.ToString();

        /// <summary>
        /// Gets user id from a claim.
        /// </summary>
        /// <param name="context">The context to find the user id from.</param>
        /// <returns>User Id.</returns>
        public static string GetUserId(this HttpContext context)
        {
            if (context.User == null)
            {
                return null;
            }

            var userId = context.User
                .FindFirst(x => x.Type.Equals("http://schemas.microsoft.com/identity/claims/objectidentifier") || x.Type.Equals("oid")).Value;

            // Default.
            return userId;
        }

        /// <summary>
        /// Get the request referer url from the header.
        /// </summary>
        /// <param name="context">The context to find the referer from.</param>
        /// <returns>string referer url.</returns>
        public static string GetRequestReferer(this HttpContext context)
        {
            return context.GetRequestHeader("Referer");
        }
    }
}
