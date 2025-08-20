using Microsoft.EntityFrameworkCore;

namespace ProGestao.Configuration
{
    /// <summary>
    /// Extensões específicas para configuração avançada
    /// </summary>
    public static class AdvancedConfigurationExtensions
    {
        /// <summary>
        /// Configura perfis de performance para diferentes ambientes
        /// </summary>
        public static IServiceCollection ConfigurePerformanceProfile(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var environment = configuration.GetValue<string>("ApplicationSettings:Environment");

            switch (environment?.ToLowerInvariant())
            {
                case "development":
                    services.ConfigureDevelopmentProfile();
                    break;
                case "staging":
                    services.ConfigureStagingProfile();
                    break;
                case "production":
                    services.ConfigureProductionProfile();
                    break;
                default:
                    services.ConfigureDefaultProfile();
                    break;
            }

            return services;
        }

        private static IServiceCollection ConfigureDevelopmentProfile(this IServiceCollection services)
        {
            // Configurações para desenvolvimento
            services.Configure<DbContextOptionsBuilder>(options =>
            {
                // Habilitar logs detalhados
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            return services;
        }

        private static IServiceCollection ConfigureStagingProfile(this IServiceCollection services)
        {
            // Configurações para staging (similar à produção mas com mais logs)
            return services;
        }

        private static IServiceCollection ConfigureProductionProfile(this IServiceCollection services)
        {
            // Configurações otimizadas para produção
            services.Configure<DbContextOptionsBuilder>(options =>
            {
                // Desabilitar logs sensíveis
                options.EnableSensitiveDataLogging(false);
            });

            return services;
        }

        private static IServiceCollection ConfigureDefaultProfile(this IServiceCollection services)
        {
            
            return services;
        }
    }
}
