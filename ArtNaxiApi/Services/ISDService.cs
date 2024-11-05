using ArtNaxiApi.Models;
using System.Net;
using System.Security.Claims;

namespace ArtNaxiApi.Services
{
    public interface ISDService
    {
        Task<(HttpStatusCode, Image?)> GenerateImageAsync(SDRequest request);
        Task<HttpStatusCode> MakeImagePublicAsync(Guid id);
        Task<HttpStatusCode> MakeImagePrivateAsync(Guid id);
        Task<HttpStatusCode> DeleteImageByIdAsync(Guid id, ClaimsPrincipal user);
    }
}