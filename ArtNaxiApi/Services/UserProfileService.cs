using ArtNaxiApi.Constants;
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

        public async Task<(HttpStatusCode, string?)> UpdateProfileAvatarByUserIdAsync(Guid userId, IFormFile avatarFile)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            var currentUserId = Guid.Parse(userIdClaim.Value);

            if (userId != currentUserId)
            {
                return (HttpStatusCode.Forbidden, null);   // Not allowed to update
            }

            if (avatarFile == null || avatarFile.Length == 0)
            {
                return (HttpStatusCode.BadRequest, null);   // No file uploaded
            }

            var request = _httpContextAccessor.HttpContext.Request;
            var schemeHost = $"{request.Scheme}://{request.Host}";

            var newFileName = $"{Guid.NewGuid()}.png";
            var newFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars", newFileName);

            var oldAvatarUrl = await _userProfileRepository.GetProfileAvatarByUserIdAsync(userId);
            var oldAvatarRelativeUrl = oldAvatarUrl.Replace(schemeHost, "");
            var oldAvatarPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldAvatarRelativeUrl.TrimStart('/'));

            if (File.Exists(oldAvatarPath))
            {
                try
                {
                    File.Delete(oldAvatarPath);
                }
                catch
                {
                    return (HttpStatusCode.BadRequest, null);
                }
            }

            using (var stream = new FileStream(newFilePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(stream);
            }

            var avatarRelativeUrl = $"/avatars/{newFileName}";
            var avatarUrl = schemeHost + avatarRelativeUrl;

            await _userProfileRepository.UpdateAvatarAsync(userId, avatarUrl);

            return (HttpStatusCode.OK, avatarUrl);
        }

        public async Task<HttpStatusCode> DeleteUserAvatarByUserIdAsync(Guid userId, ClaimsPrincipal userClaim)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            var currentUserId = Guid.Parse(userIdClaim.Value);

            if (userId != currentUserId && !userClaim.IsInRole(Roles.Admin))
            {
                return (HttpStatusCode.BadRequest);     // Not allowed to delete
            }

            var request = _httpContextAccessor.HttpContext.Request;
            var schemeHost = $"{request.Scheme}://{request.Host}";


            var userAvatarUrl = await _userProfileRepository.GetProfileAvatarByUserIdAsync(userId);
            var userAvatarRelativeUrl = userAvatarUrl.Replace(schemeHost, "");
            var userAvatarPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", userAvatarRelativeUrl.TrimStart('/'));

            if (File.Exists(userAvatarPath))
            {
                try
                {
                    File.Delete(userAvatarPath);
                }
                catch
                {
                    return (HttpStatusCode.BadRequest);
                }
            }

            string defaultAvatarUrl = _configuration["FrontendSettings:DefualtAvatarUrl"]!;
            await _userProfileRepository.UpdateAvatarAsync(userId, defaultAvatarUrl);

            return HttpStatusCode.OK;
        }

        public async Task<int> GetPublicImageCountByUserIdAsync(Guid id)
        {
            var imageCount = await _userProfileRepository.GetPublicImageCountAsync(id);

            return imageCount;
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
