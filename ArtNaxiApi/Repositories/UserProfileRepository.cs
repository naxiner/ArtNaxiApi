using ArtNaxiApi.Data;
using ArtNaxiApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ArtNaxiApi.Repositories
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly AppDbContext _context;
        public UserProfileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddProfileAsync(UserProfile profile)
        {
            await _context.UserProfiles.AddAsync(profile);
            await _context.SaveChangesAsync();
        }

        public async Task<UserProfile> GetProfileByUserIdAsync(Guid userId)
        {
            return await _context.UserProfiles
                .Include(p => p.Images)
                    .ThenInclude(i => i.User)
                .Include(p => p.Images)
                    .ThenInclude(i => i.Request)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task UpdateAsync(UserProfile profile)
        {
            _context.UserProfiles.Update(profile);
            await _context.SaveChangesAsync();
        }
    }
}
