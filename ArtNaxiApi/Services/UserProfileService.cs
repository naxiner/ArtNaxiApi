using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Repositories;
using System.Net;

namespace ArtNaxiApi.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository _userProfileRepository;
        public UserProfileService(IUserProfileRepository userProfileRepository)
        {
            _userProfileRepository = userProfileRepository;
        }

        public async Task CreateProfileAsync(Guid userId)
        {
            var profile = new UserProfile
            {
                UserId = userId,
                ProfilePictureUrl = string.Empty
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

        private UserProfileDto MapToUserProfileDto(UserProfile userProfile)
        {
            return new UserProfileDto
            {
                Id = userProfile.Id,
                Username = userProfile.User.Username,
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
