namespace ProGestao.Middleware
{
    public class ConfigurationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ConfigurationMiddleware> _logger;

        public ConfigurationMiddleware(RequestDelegate next, ILogger<ConfigurationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Adicionar headers de segurança
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Add("X-Frame-Options", "DENY");
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

            // Log de requisições em desenvolvimento
            if (context.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true)
            {
                _logger.LogDebug("📝 {Method} {Path} - {UserAgent}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.Headers.UserAgent.FirstOrDefault()?.Substring(0, 50));
            }

            await _next(context);
        }
    }
}
