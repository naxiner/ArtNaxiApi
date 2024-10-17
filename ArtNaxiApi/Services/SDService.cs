using System.Net.Http;
using System.Text.Json;
using System.Text;
using ArtNaxiApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ArtNaxiApi.Services
{
    public class SDService : ISDService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrlTextToImg;

        public SDService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiUrlTextToImg = configuration["StableDiffusion:ApiUrlTextToImg"];
        }

        public async Task<string> GenerateImageAsync(SDRequest request)
        {
            // api url text to image generation
            var urlTxt2Img = _apiUrlTextToImg;
            var jsonRequest = JsonSerializer.Serialize(request);

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(urlTxt2Img, content);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            var responseData = JsonDocument.Parse(responseBody);
            var base64Image = responseData.RootElement.GetProperty("images")[0].GetString();

            byte[] imageBytes = Convert.FromBase64String(base64Image);

            string imagePath = await SaveImage(imageBytes);
            return imagePath;
        }

        private async Task<string> SaveImage(byte[] imageBytes)
        {
            var filePath = Path.Combine("Images");
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            var fileName = $"{Guid.NewGuid()}.png";
            var fullPath = Path.Combine(filePath, fileName);

            await File.WriteAllBytesAsync(fullPath, imageBytes);
            return $"/Images/{fileName}";
        }
    }
}
