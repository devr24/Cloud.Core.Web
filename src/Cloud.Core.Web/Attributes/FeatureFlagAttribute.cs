using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Cloud.Core.Web.Attributes
{
    /// <summary>
    /// Attribute for limiting actions with a Feature Flag.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class FeatureFlagAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Key to look for feature flag.
        /// </summary>
        private readonly string _featureFlagKey;

        /// <summary>
        /// Constructor to set feature flag key
        /// </summary>
        /// <param name="featureFlagKey"></param>
        public FeatureFlagAttribute(string featureFlagKey)
        {
            _featureFlagKey = featureFlagKey;
        }

        /// <summary>
        /// Check for feature flag, only let the action continue if it is on.
        /// </summary>
        /// <param name="context">Http context currently executing.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var featureFlagService = (IFeatureFlag)context.HttpContext.RequestServices.GetService(typeof(IFeatureFlag));

            if (featureFlagService == null)
            {
                throw new InvalidOperationException("No feature flag service registered");
            }

            // Gets true/false value for supplied feature flag key, userId (which is the calling app name) can be used to determine the value as well as audit this action
            var flag = featureFlagService.GetFeatureFlag(_featureFlagKey);

            if (!flag)
            {
                context.ModelState.AddModelError("Feature", "This route has been disabled.");
                context.Result = new NotFoundObjectResult(new Validation.ValidationProblemDetails(context, System.Net.HttpStatusCode.NotFound));
            }
        }
    }
}
