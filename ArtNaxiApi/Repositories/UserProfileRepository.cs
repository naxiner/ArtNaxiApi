using ArtNaxiApi.Data;
using ArtNaxiApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
                .Include(i => i.User)
                .Include(p => p.Images)
                    .ThenInclude(i => i.User)
                .Include(p => p.Images)
                    .ThenInclude(i => i.Request)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<string?> GetProfileAvatarByUserIdAsync(Guid userId)
        {
            return await _context.UserProfiles
                .Where(p => p.UserId == userId)
                .Select(ppu => ppu.ProfilePictureUrl)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(UserProfile profile)
        {
            _context.UserProfiles.Update(profile);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAvatarAsync(Guid userId, string avatarUrl)
        {
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            userProfile.ProfilePictureUrl = avatarUrl;

            _context.UserProfiles.Update(userProfile);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetPublicImageCountAsync(Guid userId)
        {
            var userProfile = await _context.UserProfiles
                .Include(up => up.Images)
                .FirstOrDefaultAsync(up => up.UserId == userId);

            var publicImages = userProfile.Images?.Count(img => img.IsPublic) ?? 0;

            return publicImages;
        }

        public async Task DeleteUserProfileByUserIdAsync(Guid userId)
        {
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            _context.UserProfiles.Remove(userProfile);
            await _context.SaveChangesAsync();
        }
    }
}
