using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;
using System.Net;
using System.Security.Claims;

namespace ArtNaxiApi.Services
{
    public interface IUserService
    {
        Task<HttpStatusCode> RegisterUserAsync(RegistrDto model);
        Task<(HttpStatusCode, string?, string?)> LoginUserAsync(LoginDto login);
        Task<(HttpStatusCode, string?, string?)> RefreshTokenAsync(string token, string refreshToken);
        Task<HttpStatusCode> UpdateUserByIdAsync(Guid id, UpdateUserDTO model, ClaimsPrincipal userClaim);
        Task<HttpStatusCode> DeleteUserByIdAsync(Guid id, ClaimsPrincipal userClaim);
        Guid GetCurrentUserId();
        Task<User> GetUserByIdAsync(Guid id);
    }
}