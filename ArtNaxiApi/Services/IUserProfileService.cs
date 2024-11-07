using ArtNaxiApi.Models.DTO;
using System.Net;

namespace ArtNaxiApi.Services
{
    public interface IUserProfileService
    {
        Task CreateProfileAsync(Guid userId);
        Task<(HttpStatusCode, UserProfileDto?)> GetProfileByUserIdAsync(Guid userId);
        Task<(HttpStatusCode, string?)> GetProfileAvatarByUserIdAsync(Guid userId);
        Task<(HttpStatusCode, string?)> UpdateProfileAvatarByUserIdAsync(Guid userId, IFormFile avatarFile);
        Task<int> GetPublicImageCountByUserIdAsync(Guid id);
    }
}