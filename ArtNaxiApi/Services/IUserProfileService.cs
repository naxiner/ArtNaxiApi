using ArtNaxiApi.Models.DTO;
using System.Net;
using System.Security.Claims;

namespace ArtNaxiApi.Services
{
    public interface IUserProfileService
    {
        Task CreateProfileAsync(Guid userId);
        Task<(HttpStatusCode, UserProfileDto?)> GetProfileByUserIdAsync(Guid userId);
        Task<(HttpStatusCode, string?)> GetProfileAvatarByUserIdAsync(Guid userId);
        Task<(HttpStatusCode, string?)> UpdateProfileAvatarByUserIdAsync(Guid userId, IFormFile avatarFile);
        Task<HttpStatusCode> DeleteUserAvatarByUserIdAsync(Guid userId, ClaimsPrincipal userClaim);
        Task<HttpStatusCode> DeleteUserProfileByUserIdAsync(Guid userId, ClaimsPrincipal userClaim);
        Task<int> GetPublicImageCountByUserIdAsync(Guid id);
    }
}