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
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddUserAsync(user);
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
    }
}
