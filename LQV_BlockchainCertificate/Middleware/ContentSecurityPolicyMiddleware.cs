using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace LQV_BlockchainCertificate.Middleware
{
    public class ContentSecurityPolicyMiddleware
    {
        private readonly RequestDelegate _next;

        public ContentSecurityPolicyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " +
                "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
                "img-src 'self' data: blob:; " +
                "media-src 'self' blob:; " +
                "connect-src 'self' blob:;";

            await _next(context);
        }
    }
}