using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MongoAdminUI.Middleware
{
    public class TokenSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token) && !context.Request.Headers.ContainsKey("Authorization"))
            {
                context.Request.Headers.Add("Authorization", $"Bearer {token}");
            }

            await _next(context);
        }
    }
}
