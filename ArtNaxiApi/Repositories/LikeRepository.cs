using ArtNaxiApi.Data;
using ArtNaxiApi.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ArtNaxiApi.Repositories
{
    public class LikeRepository : ILikeRepository
    {
        private readonly AppDbContext _context;
        public LikeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task LikeEntityAsync(Like like)
        {
            var existingLike = await _context.Likes.FirstOrDefaultAsync(l =>
                l.UserId == like.UserId &&
                l.EntityId == like.EntityId &&
                l.EntityType == like.EntityType);

            if (existingLike != null)
            {
                return;
            }

            await _context.Likes.AddAsync(like);
            await _context.SaveChangesAsync();
        }

        public async Task DislikeEntityAsync(Like like)
        {
            var existingLike = await _context.Likes.FirstOrDefaultAsync(l =>
                l.UserId == like.UserId &&
                l.EntityId == like.EntityId &&
                l.EntityType == like.EntityType);

            if (existingLike == null)
            {
                return;
            }

            _context.Likes.Remove(existingLike);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsLikeExistsAsync(Guid userId, Guid entityId, string entityType)
        {
            return await _context.Likes.AnyAsync(l =>
                l.UserId == userId &&
                l.EntityId == entityId &&
                l.EntityType == entityType);
        }

        public async Task DeleteAllLikesByImageIdAsync(Guid imageId)
        {
            var likes = await _context.Likes
                .Where(l => l.EntityId == imageId)
                .ToListAsync();

            _context.Likes.RemoveRange(likes);
            await _context.SaveChangesAsync();
        }
    }
}
