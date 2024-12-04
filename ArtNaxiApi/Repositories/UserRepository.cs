using ArtNaxiApi.Data;
using ArtNaxiApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ArtNaxiApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync(int pageNumber, int pageSize)
        {
            return await _context.Users
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IQueryable<User>> GetUsersByQueryAsync(string query)
        {
            return _context.Users.Where(u => u.Username.Contains(query));
        }

        public async Task<int> GetTotalCountUsersAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByNameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetUserByNameOrEmailAsync(string usernameOrEmail)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);
        }

        public async Task<bool> AddUserAsync(User user)
        {
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(User user)
        {
            var userWithProfile = await _context.Users
                .Include(u => u.Profile)
                .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (userWithProfile == null)
            {
                throw new Exception("User not found");
            }

            if (userWithProfile.Profile != null)
            {
                _context.Images.RemoveRange(userWithProfile.Profile.Images);
            }

            if (userWithProfile.Profile != null)
            {
                _context.UserProfiles.Remove(userWithProfile.Profile);
            }

            _context.Users.Remove(userWithProfile);
            await _context.SaveChangesAsync();
        }
    }
}
