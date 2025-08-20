using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProGestao.Data;

namespace ProGestao.HealthChecks
{
    /// <summary>
    /// HealthCheck para verificar conectividade com banco de dados
    /// </summary>
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly ProGestaoContext _context;

        public DatabaseHealthCheck(ProGestaoContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.Database.CanConnectAsync(cancellationToken);
                return HealthCheckResult.Healthy("Database connection successful");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Database connection failed", ex);
            }
        }
    }

}
