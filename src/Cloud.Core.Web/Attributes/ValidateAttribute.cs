namespace Cloud.Core.Web.Attributes
{
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Logging;
    using System;

    /// <summary>
    /// Attribute filter that can be added to a class or method to enable
    /// model validation without the need for code.  Returns "BadRequestObjectResult" (400 response)
    /// with model state within the body.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Filters.ActionFilterAttribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        private readonly bool _logMessage;

        public ValidateModelAttribute() { }

        /// <summary>
        /// Constructor that takes a boolean indicator on whether to log the message to ILogger [true] or not.
        /// </summary>
        /// <param name="logValidationException">Boolean [true] to log validation messages and [false] (default) if it should not be logged.</param>
        public ValidateModelAttribute(bool logValidationException = false)
        {
            _logMessage = logValidationException;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var validationFailedResult = new ValidationFailedResult(context.ModelState);

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
