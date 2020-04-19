namespace Cloud.Core.Web.Middlewares
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Unhandled exception middleware monitors web requests for unexpected exceptions and handles them
    /// with an ApiErrorResponse in the body of the <see cref="HttpResponse"/> response.
    /// Logs the exceptions to the <see cref="ILogger{TCategoryName}"/> logger.
    /// </summary>
    public class UnhandledExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UnhandledExceptionMiddleware> _logger;

        public UnhandledExceptionMiddleware(RequestDelegate next, ILogger<UnhandledExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex) 
                when (ex.Message.Contains("Cannot open server"))
            {
                // Specific catch for sql related server connections.  We do this here because we DON'T want to
                // output potentially sensitive information as an exception.
                await HandleExceptionAsync(context, ex, true);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        internal Task HandleExceptionAsync(HttpContext context, Exception exception, bool applySensitiveInfoFilter = false)
        {
            // 500 if unexpected, would have been this anyway.
            var code = HttpStatusCode.InternalServerError;

            var message = exception.Message;

            // We will supress exception messages under specific conditions, so replace message with default.
            if (applySensitiveInfoFilter)
            {
                message = $"An exception occurred of type {exception.GetBaseException().GetType().Name}. Message has been surpressed, please contact support for more information.";
            }

            _logger?.LogError(exception, $"An unhandled exception has occurred while executing {context.Request.Method}");

            // ApiErrorResponse will be output.
            var apiError = new ApiErrorResult(exception, message); 

            var result = JsonConvert.SerializeObject(apiError);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            // Output result in response stream.
            return context.Response.WriteAsync(result);
        }
    }
}
