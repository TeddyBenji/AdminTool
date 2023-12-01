using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace MongoAdminUI.Filters
{
    public class AttachTokenAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var httpContext = context.HttpContext;
            var token = httpContext.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
            {
                httpContext.Items["AccessToken"] = token;
            }
        }
    }
}
