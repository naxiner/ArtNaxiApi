using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;
using System.Net;
using System.Security.Claims;

namespace ArtNaxiApi.Services
{
    public interface ISDService
    {
        Task<(HttpStatusCode, ImageDto?)> GenerateImageAsync(SDRequest request);
        Task<HttpStatusCode> MakeImagePublicAsync(Guid id);
        Task<HttpStatusCode> MakeImagePrivateAsync(Guid id);
        Task<HttpStatusCode> DeleteImageByIdAsync(Guid id, ClaimsPrincipal user);
    }
}