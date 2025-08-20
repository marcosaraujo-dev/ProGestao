using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Services;

namespace ProGestao.Configuration
{
    /// <summary>
    /// Configuração de injeção de dependência seguindo SOLID principles
    /// Centraliza registro de serviços para facilitar manutenção
    /// </summary>
    public static class DependencyInjectionConfig
    {
        /// <summary>
        /// Configura Entity Framework com pool de conexões
        /// CORREÇÃO: Pool de conexões resolve problemas de concorrência
        /// </summary>
        public static IServiceCollection AddEntityFramework(
            this IServiceCollection services,
            string connectionString)
        {
            services.AddDbContextPool<ProGestaoContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    // Configurações de resilência
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);

                    // Timeout para consultas longas
                    sqlOptions.CommandTimeout(30);
                });

                // IMPORTANTE: Pool de conexões resolve problemas de concorrência
                // Cada request recebe uma instância separada do DbContext
            }, poolSize: 128); // Ajuste conforme necessidade

            return services;
        }

        /// <summary>
        /// Registra serviços da aplicação seguindo DIP (Dependency Inversion Principle)
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ITimelineService, TimelineService>();
            
            services.AddScoped<IProjetoService, ProjetoService>();
            services.AddScoped<IAtividadeService, AtividadeService>();
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<IGridService, GridService>();
            

            return services;
        }

        /// <summary>
        /// Configura logging estruturado
        /// </summary>
        public static IServiceCollection AddStructuredLogging(this IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();

                // Em produção, considere usar Serilog ou Application Insights
                // builder.AddSerilog();
            });

            return services;
        }

        /// <summary>
        /// Configura cache para otimização de performance
        /// </summary>
        public static IServiceCollection AddCacheServices(this IServiceCollection services)
        {
            // Memory cache para dados que mudam pouco
            services.AddMemoryCache();

            // Distributed cache para aplicações multi-instância
            // services.AddStackExchangeRedisCache(options =>
            // {
            //     options.Configuration = "localhost:6379";
            // });

            return services;
        }
    }
}