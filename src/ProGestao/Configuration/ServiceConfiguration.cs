using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Services;
using ProGestao.Services.Atividades;
using ProGestao.Services.Interfaces;
using ProGestao.Services.Projetos;
using ProGestao.Services.Usuarios;

namespace ProGestao.Configuration
{
    /// <summary>
    /// Configuração ATUALIZADA de Dependency Injection para Services
    /// Usa mappers específicos por entidade
    /// </summary>
    public static class ServiceConfiguration
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            // ==============================
            // MAPPERS ESPECÍFICOS
            // ==============================
            services.AddScoped<IAtividadeMapper, AtividadeMapper>();
            services.AddScoped<IProjetoMapper, ProjetoMapper>();
            services.AddScoped<IUsuarioMapper, UsuarioMapper>();

            // ==============================
            // ATIVIDADE SERVICES
            // ==============================
            services.AddScoped<IAtividadeQueryService, AtividadeQueryService>();
            services.AddScoped<IAtividadeCommandService, AtividadeCommandService>();
            services.AddScoped<IAtividadeValidationService, AtividadeValidationService>();
           

            // ==============================
            // PROJETO SERVICES
            // ==============================
            services.AddScoped<IProjetoQueryService, ProjetoQueryService>();
            services.AddScoped<IProjetoCommandService, ProjetoCommandService>();
            services.AddScoped<IProjetoValidationService, ProjetoValidationService>();
            services.AddScoped<IProjetoLookupService, ProjetoLookupService>();


            // ==============================
            // OUTROS SERVICES
            // ==============================
            services.AddScoped<ILookupService, LookupService>();
            services.AddScoped<ITimelineService, TimelineService>();

            // Service Legacy (para compatibilidade durante transição)
            services.AddScoped<IAtividadeService, AtividadeServiceLegacy>();

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuração do DbContext
            services.AddDbContext<ProGestaoContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString);

                var environment = configuration.GetValue<string>("ApplicationSettings:Environment");
                if (environment == "Development")
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            // Business Services
            services.AddBusinessServices();

            
            services.AddScoped<GridService>();
            services.AddScoped<IGridService, GridService>();

            return services;
        }
    }


}
