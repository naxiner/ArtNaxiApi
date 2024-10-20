using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Repositories;
using System.Security.Claims;

namespace ArtNaxiApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserProfileService _userProfileService;
        private readonly IJwtService _jwtService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(
            IUserRepository userRepository,
            IUserProfileService userProfileService,
            IJwtService jwtService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _userProfileService = userProfileService;
            _jwtService = jwtService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<User> RegisterUserAsync(RegistrDto model)
        {
            if (await _userRepository.GetUserByNameAsync(model.Username) != null ||
                await _userRepository.GetUserByEmailAsync(model.Email) != null)
            {
                // User with that Username or Email already exist
                return null;
            }

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddUserAsync(user);

            await _userProfileService.CreateProfileAsync(user.Id);

            return user;
        }

        public async Task<string> LoginUserAsync(LoginDto model)
        {
            var user = await _userRepository.GetUserByNameOrEmailAsync(model.UsernameOrEmail);

            if (user == null)
            {
                // Invalid Username or Email
                return null;
            }

            var verify = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);

            if (!verify)
            {
                // Invalid Password
                return null;
            }

            var token = _jwtService.GenerateToken(user);
            return token;
        }

        public Guid GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Guid.Empty;
            }

            return Guid.Parse(userIdClaim.Value);
        }

        public async Task<bool> UpdateUserByIdAsync(Guid id, UpdateUserDTO model)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                // User not found
                return false;
            }

            var existingUser = await _userRepository.GetUserByNameAsync(model.Username);
            if (existingUser == null)
            {
                existingUser = await _userRepository.GetUserByEmailAsync(model.Email);
            }

            if (existingUser != null && existingUser.Id != id) 
            {
                return false; // Username or email already exist for another user
            }

            bool updated = false;

            if (!string.IsNullOrEmpty(model.Username) && user.Username != model.Username)
            {
                user.Username = model.Username;
                updated = true;
            }

            if (!string.IsNullOrEmpty(model.Email) && user.Email != model.Email)
            {
                user.Email = model.Email;
                updated = true;
            }

            if (!string.IsNullOrEmpty(model.Password))
            {
                user.PasswordHash = HashPassword(model.Password);
                updated = true;
            }

            if (updated)
            {
                await _userRepository.UpdateUserAsync(user);
                updated = true;
            }

            user.UpdatedAt = DateTime.UtcNow;
            return true;
        }

        public async Task<bool> DeleteUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                // User not found
                return false;
            }

            var currentUserId = GetCurrentUserId();
            if (user.Id != currentUserId) // Add a check for the Admin role here
            {
                // User does not have an Id, or is not in the Admin role
                return false;
            }

            await _userRepository.DeleteUserAsync(user);

            return true;
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
