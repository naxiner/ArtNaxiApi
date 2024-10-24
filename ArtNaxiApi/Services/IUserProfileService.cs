using ArtNaxiApi.Models;

namespace ArtNaxiApi.Services
{
    public interface IUserProfileService
    {
        Task CreateProfileAsync(Guid userId);
        Task<UserProfile> GetProfileByUserIdAsync(Guid userId);
    }
}