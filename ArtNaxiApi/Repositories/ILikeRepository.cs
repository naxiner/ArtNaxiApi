using ArtNaxiApi.Models;

namespace ArtNaxiApi.Repositories
{
    public interface ILikeRepository
    {
        Task DislikeEntityAsync(Like like);
        Task LikeEntityAsync(Like like);
        Task<bool> IsLikeExistsAsync(Guid userId, Guid entityId, string entityType);
        Task DeleteAllLikesByImageIdAsync(Guid imageId);
        Task<int> GetLikeCountByEntityIdAsync(Guid entityId);
    }
}