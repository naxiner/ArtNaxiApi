using System.Net;

namespace ArtNaxiApi.Services
{
    public interface ILikeService
    {
        Task<HttpStatusCode> LikeEntityAsync(Guid entityId, string entityType);
        Task<HttpStatusCode> DislikeEntityAsync(Guid entityId, string entityType);
    }
}