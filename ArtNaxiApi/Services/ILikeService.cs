using System.Net;

namespace ArtNaxiApi.Services
{
    public interface ILikeService
    {
        Task<(HttpStatusCode, int)> GetLikeCountByEntityIdAsync(Guid entityId);
        Task<HttpStatusCode> LikeEntityAsync(Guid entityId, string entityType);
        Task<HttpStatusCode> DislikeEntityAsync(Guid entityId, string entityType);
    }
}