using System.Text.Json;
using System.Text;
using ArtNaxiApi.Models;
using ArtNaxiApi.Repositories;
using ArtNaxiApi.Constants;
using System.Security.Claims;
using System.Net;
using ArtNaxiApi.Models.DTO;

namespace ArtNaxiApi.Services
{
    public class SDService : ISDService
    {
        private readonly HttpClient _httpClient;
        private readonly IImageRepository _imageRepository;
        private readonly IStyleRepository _styleRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IUserService _userService;
        private readonly string _apiUrlTextToImg;

        public SDService(
            HttpClient httpClient, 
            IImageRepository imageRepository,
            IStyleRepository styleRepository,
            IUserRepository userRepository,
            IUserProfileRepository userProfileRepository,
            ILikeRepository likeRepository,
            IUserService userService,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _imageRepository = imageRepository;
            _styleRepository = styleRepository;
            _userRepository = userRepository;
            _userProfileRepository = userProfileRepository;
            _likeRepository = likeRepository;
            _userService = userService;
            _apiUrlTextToImg = configuration["StableDiffusion:ApiUrlTextToImg"];
        }

        public async Task<(HttpStatusCode, ImageDto?)> GenerateImageAsync(SDRequest request)
        {
            // api url text to image generation
            var urlTxt2Img = _apiUrlTextToImg;
            var jsonRequest = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            HttpResponseMessage response;
            
            try
            {
                response = await _httpClient.PostAsync(urlTxt2Img, content);
            }
            catch
            {
                return (HttpStatusCode.ServiceUnavailable, null);
            }

            var userId = _userService.GetCurrentUserId();
            var currentUser = await _userRepository.GetUserByIdAsync(userId);
            var userProfile = await _userProfileRepository.GetProfileByUserIdAsync(userId);

            string responseBody = await response.Content.ReadAsStringAsync();

            var responseData = JsonDocument.Parse(responseBody);
            var base64Image = responseData.RootElement.GetProperty("images")[0].GetString();

            byte[] imageBytes = Convert.FromBase64String(base64Image);

            string imagePath;

            try
            {
                imagePath = await SaveImage(imageBytes);
            }
            catch
            {
                return (HttpStatusCode.InternalServerError, null);
            }

            var styleEntities = new List<Style>();
            foreach (var styleName in request.Styles)
            {
                var style = await _styleRepository.GetStyleByNameAsync(styleName);

                if (style != null)
                {
                    styleEntities.Add(style);
                }
            }

            request.SDRequestStyles = styleEntities
                .Select(style => new SDRequestStyle { SDRequestId = request.Id, StyleId = style.Id })
                .ToList();

            var image = new Image
            {
                Url = imagePath,
                CreationTime = DateTime.UtcNow,
                CreatedBy = currentUser.Username,
                Request = request,
                IsPublic = false,
                User = currentUser,
                UserId = userId
            };

            userProfile.Images.Add(image);
            userProfile.UpdatedAt = DateTime.UtcNow;

            await _imageRepository.AddImageAsync(image);
            await _userProfileRepository.UpdateAsync(userProfile);

            var requestDto = new SDRequestDto
            {
                Id = request.Id,
                Prompt = request.Prompt,
                NegativePrompt = request.NegativePrompt,
                Styles = request.Styles,
                Seed = request.Seed,
                SamplerName = request.SamplerName,
                Scheduler = request.Scheduler,
                Steps = request.Steps,
                CfgScale = request.CfgScale,
                Width = request.Width,
                Height = request.Height
            };

            var imageDto = new ImageDto
            {
                Id = image.Id,
                Url = image.Url,
                CreationTime = image.CreationTime,
                CreatedBy = image.CreatedBy,
                IsPublic = image.IsPublic,
                UserId = image.UserId,
                Request = requestDto
            };

            return (HttpStatusCode.OK, imageDto);
        }

        public async Task<HttpStatusCode> DeleteImageByIdAsync(Guid id, ClaimsPrincipal user)
        {
            var image = await _imageRepository.GetImageByIdAsync(id);
            if (image == null)
            {
                return HttpStatusCode.NotFound;
            }

            var currentUserId = _userService.GetCurrentUserId();
            if (image.UserId != currentUserId && !user.IsInRole(Roles.Admin))
            {
                return HttpStatusCode.Forbidden;
            }

            await _imageRepository.DeleteImageByIdAsync(id);
            await _likeRepository.DeleteAllLikesByImageIdAsync(image.Id);

            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var filePath = Path.Combine(webRootPath, image.Url.TrimStart('/')); 

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return HttpStatusCode.NoContent;
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
