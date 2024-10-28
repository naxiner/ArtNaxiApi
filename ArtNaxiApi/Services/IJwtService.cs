using ArtNaxiApi.Models;
using System.Security.Claims;

namespace ArtNaxiApi.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();
        Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        void RemoveRefreshTokenFromCookie();
        void SetRefreshTokenInCookie(string refreshToken);
    }
}