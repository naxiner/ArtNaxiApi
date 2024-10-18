using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Repositories;

namespace ArtNaxiApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public UserService(
            IUserRepository userRepository,
            IJwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<User> RegisterUserAsync(RegistrDto model)
        {
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddUserAsync(user);
            return user;
        }

        public async Task<string> LoginUserAsync(LoginDto model)
        {
            var user = await _userRepository.GetUserByNameAsync(model.Username);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var verify = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);

            if (!verify)
            {
                throw new Exception("Failed to login.");
            }

            var token = _jwtService.GenerateToken(user);
            return token;
        }
    }
}
