using ProGestao.Middleware;

namespace ProGestao.Configuration
{
    /// <summary>
    /// Extensão para configuração completa da aplicação refatorada
    /// </summary>
    public static class CompleteConfigurationExtensions
    {
        public static WebApplication ConfigureCompleteApplication(this WebApplication app)
        {
            var environment = app.Environment;
            var logger = app.Services.GetRequiredService<ILogger<Program>>();

            // Middleware pipeline otimizado
            if (environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.Use(async (context, next) =>
                {
                    // Middleware para debug em desenvolvimento
                    logger.LogDebug("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
                    await next();
                });
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // Middleware personalizado
            app.UseMiddleware<PerformanceMonitoringMiddleware>();
            app.UseMiddleware<CustomExceptionHandlingMiddleware>();

            // Pipeline padrão
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            // Health checks endpoint
            app.MapHealthChecks("/health");

            // Razor Pages
            app.MapRazorPages();

            logger.LogInformation("Pipeline de middleware configurado para ambiente: {Environment}", environment.EnvironmentName);

            return app;
        }
    }
}
