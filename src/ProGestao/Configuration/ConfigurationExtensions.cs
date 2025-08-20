using Microsoft.EntityFrameworkCore;
using ProGestao.Data;

namespace ProGestao.Configuration
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddSecureConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // Validar configuração na inicialização
            ConfigurationValidator.ValidateConfiguration(configuration);

            // Configurar ApplicationSettings
            services.Configure<ApplicationSettings>(configuration.GetSection("ApplicationSettings"));

           
            return services;
        }

        public static WebApplication ConfigureSecureApp(this WebApplication app)
        {
            var configuration = app.Configuration;
            var appSettings = configuration.GetSection("ApplicationSettings").Get<ApplicationSettings>();

            // Configurar tratamento de erros baseado no ambiente
            if (appSettings?.Environment == "Development" && appSettings.EnableDetailedErrors)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // Middleware de segurança
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            return app;
        }
    }
}