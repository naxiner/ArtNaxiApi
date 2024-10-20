using ArtNaxiApi.Models;
using ArtNaxiApi.Repositories;

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
    }
}
