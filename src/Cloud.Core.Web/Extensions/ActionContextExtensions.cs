namespace Microsoft.AspNetCore.Mvc
{
    public static class ActionContextExtensions
    {
        public static string ActionName(this ActionContext ctx)
        {
            string actionName = ctx.RouteData.Values["action"].ToString();
            return actionName;
        }

        public static string ControllerName(this ActionContext ctx)
        {
            string controllerName = ctx.RouteData.Values["controller"].ToString();
            return controllerName + "Controller";
        }
    }
}
