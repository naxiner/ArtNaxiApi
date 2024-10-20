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
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly string _apiUrlTextToImg;

        public SDService(
            HttpClient httpClient, 
            IImageRepository imageRepository,
            IUserRepository userRepository,
            IUserService userService,
            IUserProfileRepository userProfileRepository,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _imageRepository = imageRepository;
            _userRepository = userRepository;
            _userService = userService;
            _userProfileRepository = userProfileRepository;
            _apiUrlTextToImg = configuration["StableDiffusion:ApiUrlTextToImg"];
        }

        public async Task<string> GenerateImageAsync(Guid userId, SDRequest request)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            var userProfile = await _userProfileRepository.GetProfileByUserIdAsync(userId);

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
                Request = request,
                User = user,
                UserId = userId
            };

            userProfile.Images.Add(image);

            await _imageRepository.AddImageAsync(image);

            return imagePath;
        }

        public async Task<bool> DeleteImageByIdAsync(Guid id)
        {
            var image = await _imageRepository.GetImageByIdAsync(id);
            if (image == null)
            {
                return false;
            }

            var currentUserId = _userService.GetCurrentUserId();

            if (image.UserId != currentUserId)
            {
                return false;
            }

            await _imageRepository.DeleteImageByIdAsync(id);

            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var filePath = Path.Combine(webRootPath, image.Url.TrimStart('/')); 

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return true;
        }

        private async Task<string> SaveImage(byte[] imageBytes)
        {
            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var imagesFolder = Path.Combine(webRootPath, "Images");

            if (!Directory.Exists(imagesFolder))
            {
                Directory.CreateDirectory(imagesFolder);
            }

            var fileName = $"{Guid.NewGuid()}.png";
            var fullPath = Path.Combine(imagesFolder, fileName);

            await File.WriteAllBytesAsync(fullPath, imageBytes);
            return $"/Images/{fileName}";
        }
    }
}
