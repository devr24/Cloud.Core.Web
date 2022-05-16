namespace Cloud.Core.Web.Validation
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Logging;
    using System;

    /// <summary>
    /// Attribute filter that can be added to a class or method to enable
    /// model validation without the need for code.  Returns "BadRequestObjectResult" (400 response)
    /// with model state within the body.
    /// </summary>
    /// <seealso cref="ActionFilterAttribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        private readonly bool _logMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateModelAttribute"/> class.
        /// </summary>
        public ValidateModelAttribute() { }

        /// <summary>
        /// Constructor that takes a boolean indicator on whether to log the message to ILogger [true] or not.
        /// </summary>
        /// <param name="logValidationException">Boolean [true] to log validation messages and [false] (default) if it should not be logged.</param>
        public ValidateModelAttribute(bool logValidationException = false)
        {
            _logMessage = logValidationException;
        }

        /// <summary>
        /// Called when [action executing].
        /// </summary>
        /// <param name="context">The context.</param>
        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var validationFailedResult = new BadRequestObjectResult(new ValidationProblemDetails(context, System.Net.HttpStatusCode.BadRequest));

                if (_logMessage)
                {
                    var logger = (ILogger)context.HttpContext.RequestServices.GetService(typeof(ILogger));
                    logger?.LogInformation(validationFailedResult.ToString());
                }

                context.Result = validationFailedResult;
            }
        }
    }
}
