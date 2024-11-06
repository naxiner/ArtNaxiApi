using ArtNaxiApi.Models.DTO;
using System.Net;

namespace ArtNaxiApi.Services
{
    public interface IUserProfileService
    {
        Task CreateProfileAsync(Guid userId);
        Task<(HttpStatusCode, UserProfileDto?)> GetProfileByUserIdAsync(Guid userId);
        Task<(HttpStatusCode, string?)> GetProfileAvatarByUserIdAsync(Guid userId);
        Task<HttpStatusCode> UpdateProfileAvatarByUserIdAsync(Guid userId, IFormFile avatarFile);
    }
}