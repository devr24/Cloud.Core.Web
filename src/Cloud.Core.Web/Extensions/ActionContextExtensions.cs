namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Class Action Context extensions.
    /// </summary>
    public static class ActionContextExtensions
    {
        /// <summary>
        /// Actions the name.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <returns>System.String.</returns>
        public static string ActionName(this ActionContext ctx)
        {
            string actionName = ctx.RouteData.Values["action"].ToString();
            return actionName;
        }

        /// <summary>
        /// Controllers the name.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <returns>System.String.</returns>
        public static string ControllerName(this ActionContext ctx)
        {
            string controllerName = ctx.RouteData.Values["controller"].ToString();
            return controllerName + "Controller";
        }
    }
}
