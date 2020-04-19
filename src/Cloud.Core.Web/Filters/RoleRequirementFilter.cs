using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Cloud.Core.Web.Filters
{
    /// <summary>
    /// A filter that checks roles.
    /// Do not use filter directly, instead use the RoleRequirementAttribute which will be handled by this filter.
    /// </summary>
    public class RoleRequirementFilter : IAuthorizationFilter
    {
        private readonly string[] _roles;
        private readonly IFeatureFlag _featureFlags;

        private const string FeatureFlagName = "RolesBasedAuthentication";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="roles">Roles to check.</param>
        /// <param name="featureFlags">Optional Feature Flag service.</param>
        public RoleRequirementFilter(string[] roles, IFeatureFlag featureFlags = null)
        {
            _roles = roles;
            _featureFlags = featureFlags;
        }

        /// <summary>
        /// Custom Authorization logic
        /// </summary>
        /// <param name="context"></param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            IEnumerable<Claim> userRoles;

            // Get a list of claims - this will work if there is no user identity in the current context.
            if (context.HttpContext.User.IsNullOrDefault())
            {
                userRoles = new List<Claim>();
            }
            else
            {
                userRoles = context.HttpContext.User.FindAll(x => x.Type.Equals(ClaimTypes.Role));
            }

            // If feature flags services exists, ensure RolesBasedAuthentication is on.
            if (_featureFlags != null)
            {
                //oid from bearer token
                var isRolesBasedAuthOn = _featureFlags.GetFeatureFlag(FeatureFlagName);

                if (isRolesBasedAuthOn)
                {
                    var userHasRole = _roles.Any(x => userRoles.Any(c => c.Value.Equals(x)));

                    if (!userHasRole)
                    {
                        context.Result = new ForbidResult();
                    }
                }
            }
            else
            {
                var userHasRole = _roles.Any(x => userRoles.Any(c => c.Value.Equals(x)));

                if (!userHasRole)
                {
                    context.Result = new ForbidResult();
                }
            }
        }
    }
}
