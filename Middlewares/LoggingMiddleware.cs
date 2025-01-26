using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FinalMarzo.net.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            // Log de la solicitud
            Console.WriteLine($"Incoming request: {context.Request.Method} {context.Request.Path}");

            await _next(context); // Llamar al siguiente middleware

            // Log de la respuesta
            stopwatch.Stop();
            Console.WriteLine(
                $"Response: {context.Response.StatusCode} in {stopwatch.ElapsedMilliseconds}ms"
            );
        }
    }
}
