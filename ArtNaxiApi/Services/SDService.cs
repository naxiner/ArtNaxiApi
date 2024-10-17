using System.Text.Json;
using System.Text;
using ArtNaxiApi.Models;
using ArtNaxiApi.Repositories;

namespace ArtNaxiApi.Services
{
    public class SDService : ISDService
    {
        private readonly HttpClient _httpClient;
        private readonly IImageRepository _imageRepository;
        private readonly string _apiUrlTextToImg;

        public SDService(
            HttpClient httpClient, 
            IImageRepository imageRepository,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _imageRepository = imageRepository;
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

            var image = new Image
            {
                Url = imagePath,
                CreationTime = DateTime.Now,
                Request = request
            };

            await _imageRepository.AddImageAsync(image);

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
