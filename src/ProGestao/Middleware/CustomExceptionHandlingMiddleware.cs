namespace ProGestao.Middleware
{
    /// <summary>
    /// Middleware para tratamento de exceções customizado
    /// </summary>
    public class CustomExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionHandlingMiddleware> _logger;

        public CustomExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<CustomExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred. Path: {Path}", context.Request.Path);

                // Redirecionar para página de erro apropriada
                if (!context.Response.HasStarted)
                {
                    context.Response.Redirect("/Error");
                }
            }
        }
    }
}
