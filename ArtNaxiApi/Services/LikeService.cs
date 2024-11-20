using ArtNaxiApi.Models;
using ArtNaxiApi.Repositories;
using ArtNaxiApi.Validation;
using System.Net;

namespace ArtNaxiApi.Services
{
    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IUserService _userService;

        public LikeService(ILikeRepository likeRepository, IUserService userService)
        {
            _likeRepository = likeRepository;
            _userService = userService;
        }

        public async Task<(HttpStatusCode, int)> GetLikeCountByEntityIdAsync(Guid entityId)
        {
            var likeCount = await _likeRepository.GetLikeCountByEntityIdAsync(entityId);

            return (HttpStatusCode.OK, likeCount);
        }

        public async Task<(HttpStatusCode, bool)> GetLikeStatusAsync(Guid userId, Guid entityId, string entityType)
        {
            var isLiked = await _likeRepository.IsLikeExistsAsync(userId, entityId, entityType);

            return (HttpStatusCode.OK, isLiked);
        }

        public async Task<HttpStatusCode> LikeEntityAsync(Guid entityId, string entityType)
        {
            if (!EntityTypeValidator.IsValidEntity(entityType))
            {
                return HttpStatusCode.BadRequest;           // invalid entity type
            }

            var userId = _userService.GetCurrentUserId();

            var existingLike = await _likeRepository.IsLikeExistsAsync(userId, entityId, entityType);

            if (existingLike) 
            {
                return HttpStatusCode.Conflict;             // like already exist
            }

            var like = new Like
            {
                UserId = userId,
                EntityId = entityId,
                EntityType = entityType
            };

            await _likeRepository.LikeEntityAsync(like);
            
            return HttpStatusCode.OK;
        }

        public async Task<HttpStatusCode> DislikeEntityAsync(Guid entityId, string entityType)
        {
            if (!EntityTypeValidator.IsValidEntity(entityType))
            {
                return HttpStatusCode.BadRequest;           // invalid entity type
            }

            var userId = _userService.GetCurrentUserId();

            var existingLike = await _likeRepository.IsLikeExistsAsync(userId, entityId, entityType);

            if (!existingLike)
            {
                return HttpStatusCode.NotFound;           // like not exist
            }

            var like = new Like
            {
                UserId = userId,
                EntityId = entityId,
                EntityType = entityType
            };

            await _likeRepository.DislikeEntityAsync(like);

            return HttpStatusCode.OK;
        }
    }
}
