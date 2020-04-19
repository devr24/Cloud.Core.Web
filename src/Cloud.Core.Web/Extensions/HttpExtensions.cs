namespace System.Web.Http
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;
    using Linq;
    using Text;

    /// <summary>
    /// Extension methods for the HttpRequest and HttpResponse objects.
    /// </summary>
    public static class HttpExtension
    {
        /// <summary>
        /// Http request information in a formatted string.
        /// </summary>
        /// <param name="req">The request to build the string from.</param>
        /// <returns>Formatted string information about the request.</returns>
        public static string ToFormattedString(this HttpRequest req)
        {
            // Otherwise, build logging information about the request.
            var sb = new StringBuilder("Request information: ");

            var refer = req.Headers["Referer"];

            // Get the request information for logging to file.
            sb.AppendFormat("\nHeaders: {0}", string.Join(", ", req.Headers.Select(d =>
                $"{d.Key}:{(d.Key == "Cookie" ? new StringValues(d.Value.ToString().Split('=')[0] + "=<set>") : d.Value)}")));
            sb.AppendFormat("\nHostname: {0}", req.Host.Value);
            sb.AppendFormat("\nReferer: {0}", refer.Count > 0 ? refer.ToString() : string.Empty);
            sb.AppendFormat("\nPath: {0}", req.Path);
            sb.AppendFormat("\nMethod: {0}", req.Method);
            sb.AppendFormat("\nContentType: {0}", req.ContentType);
            sb.AppendFormat("\nUser: {0}", req.HttpContext.User?.Identity.Name);
            sb.AppendFormat("\nAuthenticated: {0}", req.HttpContext.User?.Identity.IsAuthenticated);
            sb.AppendFormat("\nAuthType: {0}", req.HttpContext.User?.Identity.AuthenticationType);

            return sb.ToString();
        }

        /// <summary>
        /// Http response information in a formatted string.
        /// </summary>
        /// <param name="res">The response to build the string from.</param>
        /// <returns>Formatted string information about the response.</returns>
        public static string ToFormattedString(this HttpResponse res)
        {
            // Otherwise, build logging information about the request.
            var sb = new StringBuilder("Response information: ");

            // Get the request information for logging to file.
            sb.AppendFormat("\nHeaders: {0}", string.Join(", ", res.Headers.Select(d =>
                $"{d.Key}:{(d.Key == "Cookie" ? new StringValues(d.Value.ToString().Split('=')[0] + "=<set>") : d.Value)}")));
            sb.AppendFormat("\nBody: {0}", res.Body);
            sb.AppendFormat("\nStatusCode: {0}", res.StatusCode);
            sb.AppendFormat("\nContentType: {0}", res.ContentType);
            sb.AppendFormat("\nUser: {0}", res.HttpContext.User.Identity.Name);
            sb.AppendFormat("\nAuthenticated: {0}", res.HttpContext.User?.Identity.IsAuthenticated);
            sb.AppendFormat("\nAuthType: {0}", res.HttpContext.User?.Identity.AuthenticationType);

            return sb.ToString();
        }
    }
}
