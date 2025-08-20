using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProGestao.Services.Interfaces;

namespace ProGestao.HealthChecks
{
    /// <summary>
    /// HealthCheck para verificar services críticos
    /// </summary>
    public class ServicesHealthCheck : IHealthCheck
    {
        private readonly IServiceProvider _serviceProvider;

        public ServicesHealthCheck(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                // Verificar services críticos
                var queryService = scope.ServiceProvider.GetRequiredService<IAtividadeQueryService>();
                var commandService = scope.ServiceProvider.GetRequiredService<IAtividadeCommandService>();

                return HealthCheckResult.Healthy("All critical services are available");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Critical services unavailable", ex);
            }
        }
    }

}
