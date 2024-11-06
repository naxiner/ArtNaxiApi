using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Repositories;
using System.Net;
using System.Security.Claims;

namespace ArtNaxiApi.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserProfileService(
            IUserProfileRepository userProfileRepository,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _userProfileRepository = userProfileRepository;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task CreateProfileAsync(Guid userId)
        {
            string defaultAvatarUrl = _configuration["FrontendSettings:DefualtAvatarUrl"]!;

            var profile = new UserProfile
            {
                UserId = userId,
                ProfilePictureUrl = defaultAvatarUrl
            };

            await _userProfileRepository.AddProfileAsync(profile);
        }

        public async Task<(HttpStatusCode, UserProfileDto?)> GetProfileByUserIdAsync(Guid userId)
        {
            var userProfile = await _userProfileRepository.GetProfileByUserIdAsync(userId);
            if (userProfile == null)
            {
                return (HttpStatusCode.NotFound, null);
            }

            var userProfileDto = MapToUserProfileDto(userProfile);
            
            return (HttpStatusCode.OK, userProfileDto);
        }

        public async Task<(HttpStatusCode, string?)> GetProfileAvatarByUserIdAsync(Guid userId)
        {
            var userAvatar = await _userProfileRepository.GetProfileAvatarByUserIdAsync(userId);
            
            if (userAvatar == null)
            {
                userAvatar = _configuration["FrontendSettings:DefualtAvatarUrl"]!;
                return (HttpStatusCode.NotFound, userAvatar);
            }

            return (HttpStatusCode.OK, userAvatar);
        }

        public async Task<HttpStatusCode> UpdateProfileAvatarByUserIdAsync(Guid userId, IFormFile avatarFile)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            var currentUserId = Guid.Parse(userIdClaim.Value);

            if (userId != currentUserId)
            {
                return HttpStatusCode.Forbidden;   // Not allowed to update
            }

            if (avatarFile == null || avatarFile.Length == 0)
            {
                return HttpStatusCode.BadRequest;   // No file uploaded
            }

            var fileName = $"{userId}.png";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(stream);
            }

            var avatarRelativeUrl = $"/avatars/{fileName}";

            var request = _httpContextAccessor.HttpContext.Request;
            var avatarUrl = $"{request.Scheme}://{request.Host}{avatarRelativeUrl}";
            await _userProfileRepository.UpdateAvatarAsync(userId, avatarUrl);

            return HttpStatusCode.OK;
        }

        private UserProfileDto MapToUserProfileDto(UserProfile userProfile)
        {
            return new UserProfileDto
            {
                Id = userProfile.Id,
                Username = userProfile.User.Username,
                Email = userProfile.User.Email,
                ProfilePictureUrl = userProfile.ProfilePictureUrl,
                Images = userProfile.Images.Select(image => new ImageDto
                {
                    Id = image.Id,
                    Url = image.Url,
                    CreationTime = image.CreationTime
                }).ToList()
            };
        }
    }
}
