using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;
using System.Net;
using System.Security.Claims;

namespace ArtNaxiApi.Services
{
    public interface IUserService
    {
        Task<(HttpStatusCode, string?)> RegisterUserAsync(RegistrDto model);
        Task<(HttpStatusCode, string?, string?)> LoginUserAsync(LoginDto login);
        Task<(HttpStatusCode, string?, string?)> RefreshTokenAsync();
        Task<HttpStatusCode> UpdateUserByIdAsync(Guid id, UpdateUserDTO model, ClaimsPrincipal userClaim);
        Task<HttpStatusCode> DeleteUserByIdAsync(Guid id, ClaimsPrincipal userClaim);
        Guid GetCurrentUserId();
        Task<(HttpStatusCode, IEnumerable<UserDto>?, int)> GetAllUsersAsync(ClaimsPrincipal userClaim, int pageNumber, int pageSize);
        Task<User> GetUserByIdAsync(Guid id);
        Task<HttpStatusCode> BanUnbanUserByIdAsync(Guid id, bool isBanned, ClaimsPrincipal userClaim);
    }
}