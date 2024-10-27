using ArtNaxiApi.Constants;
using ArtNaxiApi.Models;
using System.Security.Claims;

namespace ArtNaxiApi.Services
{
    public interface ISDService
    {
        Task<string> GenerateImageAsync(Guid userId, SDRequest request);
        Task<ResultCode> DeleteImageByIdAsync(Guid id, ClaimsPrincipal user);
    }
}