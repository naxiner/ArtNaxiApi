using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;

namespace ArtNaxiApi.Services
{
    public interface IUserProfileService
    {
        Task CreateProfileAsync(Guid userId);
        Task<UserProfile> GetProfileByUserIdAsync(Guid userId);
        UserProfileDto MapToUserProfileDto(UserProfile userProfile);
    }
}