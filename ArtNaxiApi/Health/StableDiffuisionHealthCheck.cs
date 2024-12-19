using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ArtNaxiApi.Health
{
    public class StableDiffusionHealthCheck : IHealthCheck
    {
        private readonly string _apiUrlTextToImg;
        private readonly HttpClient _httpClient;

        public StableDiffusionHealthCheck(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _apiUrlTextToImg = configuration["StableDiffusion:ApiUrlTextToImg"]!;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = new())
        {
            try
            
            {
                var response = await _httpClient.GetAsync(_apiUrlTextToImg, cancellationToken);

                // Check if server is running should returns 405
                if (response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed)
                {
                    return HealthCheckResult.Healthy("StableDiffusion is accessible.");
                }

                return HealthCheckResult.Unhealthy($"StableDiffusion returned not 405 code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("StableDiffusion is not accessible.", ex);
            }
        }
    }
}
