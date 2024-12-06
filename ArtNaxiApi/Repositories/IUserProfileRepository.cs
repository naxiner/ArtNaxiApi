using ArtNaxiApi.Models;
using System.Net;

namespace ArtNaxiApi.Repositories
{
    public interface IUserProfileRepository
    {
        Task AddProfileAsync(UserProfile profile);
        Task<UserProfile?> GetProfileByUserIdAsync(Guid userId);
        Task<string?> GetProfileAvatarByUserIdAsync(Guid userId);
        Task UpdateAsync(UserProfile profile);
        Task UpdateAvatarAsync(Guid userId, string avatarUrl);
        Task<int> GetAllImageCountAsync(Guid userId);
        Task<int> GetPublicImageCountAsync(Guid userId);
        Task DeleteUserProfileByUserIdAsync(Guid userId);
    }
}