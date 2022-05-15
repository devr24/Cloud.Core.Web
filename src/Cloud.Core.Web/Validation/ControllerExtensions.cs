using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace Cloud.Core.Web.Validation
{
    /// <summary>
    /// Validation problem info extensions.
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Add error with code to modelstate dictionary.
        /// </summary>
        /// <param name="stateDictionary">Model state dictionary.</param>
        /// <param name="key">Key for the model state item.</param>
        /// <param name="message">Error message.</param>
        /// <param name="errorCode">Error code.</param>
        /// <returns></returns>
        public static ModelStateDictionary AddErrorWithCode(this ModelStateDictionary stateDictionary, string key, string message, string errorCode)
        {
            stateDictionary.AddModelError(key, $"{errorCode}|{message}");
            return stateDictionary;
        }

        /// <summary>
        /// Create a Validation error result from the controller.
        /// </summary>
        /// <param name="controllerBase">Controller to extend.</param>
        /// <returns>IActionResult.</returns>
        public static IActionResult ValidationErrorResult(this ControllerBase controllerBase)
        {
            var apiBehaviorOptions = controllerBase.HttpContext.RequestServices.GetService(typeof(IOptions<ApiBehaviorOptions>)) as IOptions<ApiBehaviorOptions>;

            if (apiBehaviorOptions == null)
                return new BadRequestObjectResult(new ValidationProblemDetails(controllerBase.ModelState));

            return apiBehaviorOptions.Value.InvalidModelStateResponseFactory(controllerBase.ControllerContext);
        }
    }
}
