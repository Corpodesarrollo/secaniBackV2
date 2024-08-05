using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Core.Validators.MSPermisos
{
    public class CustomHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            bool healthCheckResultHealthy = true;

            if (healthCheckResultHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy("The custom check indicates a healthy result."));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("The custom check indicates an unhealthy result."));
        }
    }
}
