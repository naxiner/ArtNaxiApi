using ArtNaxiApi.Models.DTO;
using System.Net;
using System.Security.Claims;

namespace ArtNaxiApi.Services
{
    public interface IStyleService
    {
        Task<(HttpStatusCode, StyleDto?)> GetStyleByIdAsync(Guid id);
        Task<(HttpStatusCode, StyleDto?)> GetStyleByNameAsync(string styleName);
        Task<(HttpStatusCode, IEnumerable<StyleDto>?, int)> GetAllStylesAsync(int pageNumber, int pageSize);
        Task<(HttpStatusCode, int)> GetTotalStylesCountAsync();
        Task<HttpStatusCode> AddStyleAsync(AddStyleDto style, ClaimsPrincipal userClaim);
        Task<HttpStatusCode> UpdateStyleByIdAsync(Guid id, StyleDto styleDto, ClaimsPrincipal userClaim);
        Task<HttpStatusCode> DeleteStyleByIdAsync(Guid id, ClaimsPrincipal userClaim);
    }
}