using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace ArtNaxiApi.Health
{
    public class RedisHealthCheck : IHealthCheck
    {
        private readonly string _redisConnectionString;

        public RedisHealthCheck(IConfiguration configuration)
        {
            _redisConnectionString = configuration.GetConnectionString("Redis")!;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, 
            CancellationToken cancellationToken = new())
        {
            try
            {
                using var connection = await ConnectionMultiplexer.ConnectAsync(_redisConnectionString);
                var database = connection.GetDatabase();
                var pingResult = await database.PingAsync();

                return HealthCheckResult.Healthy("Redis is accessible.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Redis is not accessible.", ex);
            }
        }
    }
}
