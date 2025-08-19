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

            // Configurar DbContext
            services.AddDbContext<ProGestaoContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString);

                // Configurar logging SQL baseado na configuração
                var appSettings = configuration.GetSection("ApplicationSettings").Get<ApplicationSettings>();
                if (appSettings?.EnableSqlLogging == true)
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

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