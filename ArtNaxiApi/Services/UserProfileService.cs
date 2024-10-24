using ArtNaxiApi.Models;
using ArtNaxiApi.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;

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

        public async Task<UserProfile> GetProfileByUserIdAsync(Guid userId)
        {
            return await _userProfileRepository.GetProfileByUserIdAsync(userId);
        }
    }
}
