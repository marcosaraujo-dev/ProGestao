using ProGestao.Services;

namespace ProGestao.Extensions
{
    public static class GridServiceExtensions
    {
        /// <summary>
        /// Registra serviços do Grid no DI container
        /// </summary>
        public static IServiceCollection AddGridServices(this IServiceCollection services)
        {
            services.AddScoped<IGridService, GridService>();

            return services;
        }
    }
}
