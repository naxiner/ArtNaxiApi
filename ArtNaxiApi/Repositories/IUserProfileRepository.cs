using ArtNaxiApi.Models;

namespace ArtNaxiApi.Repositories
{
    public interface IUserProfileRepository
    {
        Task AddProfileAsync(UserProfile profile);
        Task<UserProfile> GetProfileByUserIdAsync(Guid userId);
        Task<string?> GetProfileAvatarByUserIdAsync(Guid userId);
        Task UpdateAsync(UserProfile profile);
    }
}