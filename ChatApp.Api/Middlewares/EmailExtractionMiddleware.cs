using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ChatApp.Api.Middlewares
{
    public class EmailExtractionMiddleware
    {
        private readonly RequestDelegate _next;

        public EmailExtractionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var emailClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (emailClaim != null)
                {
                    // Store email in HttpContext items for easy access by controllers
                    context.Items["UserEmail"] = emailClaim.Value;
                }
            }

            await _next(context);
        }
    }
}
