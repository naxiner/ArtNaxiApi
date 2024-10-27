using ArtNaxiApi.Models.DTO;
using System.Net;

namespace ArtNaxiApi.Services
{
    public interface IUserProfileService
    {
        Task CreateProfileAsync(Guid userId);
        Task<(HttpStatusCode, UserProfileDto?)> GetProfileByUserIdAsync(Guid userId);
    }
}