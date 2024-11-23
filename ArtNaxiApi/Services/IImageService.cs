using ArtNaxiApi.Models.DTO;
using System.Net;
using System.Security.Claims;

namespace ArtNaxiApi.Services
{
    public interface IImageService
    {
        Task<(HttpStatusCode, IEnumerable<ImageDto>, int)> GetAllImagesAsync(int pageNumber, int pageSize, ClaimsPrincipal userClaim);
        Task<(HttpStatusCode, ImageDto?)> GetImageByIdAsync(Guid id);
        Task<(HttpStatusCode, IEnumerable<ImageDto>, int)> GetImagesByUserIdAsync(Guid userId, int pageNumber, int pageSize, ClaimsPrincipal userClaim);
        Task<(HttpStatusCode, IEnumerable<ImageDto>, int)> GetPopularPublicImagesAsync(int pageNumber, int pageSize);
        Task<(HttpStatusCode, IEnumerable<ImageDto>, int)> GetPublicImagesByUserIdAsync(Guid userId, int pageNumber, int pageSize);
        Task<(HttpStatusCode, IEnumerable<ImageDto>, int)> GetRecentImagesAsync(int pageNumber, int pageSize);
        Task<(HttpStatusCode, IEnumerable<ImageDto>, int)> GetRecentPublicImagesAsync(int pageNumber, int pageSize);
        Task<HttpStatusCode> MakeImagePrivateAsync(Guid id);
        Task<HttpStatusCode> MakeImagePublicAsync(Guid id);
    }
}