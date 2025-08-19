using ProGestao.Services;

namespace ProGestao.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Registrar todos os serviços da aplicação
     
            services.AddScoped<IProjetoService, ProjetoService>();
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<IEquipeService, EquipeService>();
            services.AddScoped<IStatusService, StatusService>();
            services.AddScoped<IAtividadeService, AtividadeService>();

            return services;
        }
    }
}
