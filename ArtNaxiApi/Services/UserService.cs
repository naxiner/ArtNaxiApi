using ArtNaxiApi.Constants;
using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Repositories;
using ArtNaxiApi.Validation;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;

namespace ArtNaxiApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IImageRepository _imageRepository;
        private readonly IUserProfileService _userProfileService;
        private readonly IJwtService _jwtService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(
            IUserRepository userRepository,
            IImageRepository imageRepository,
            IUserProfileService userProfileService,
            IJwtService jwtService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _imageRepository = imageRepository;
            _userProfileService = userProfileService;
            _jwtService = jwtService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(HttpStatusCode, string?)> RegisterUserAsync(RegistrDto model)
        {
            if (await _userRepository.GetUserByNameAsync(model.Username) != null ||
                await _userRepository.GetUserByEmailAsync(model.Email) != null)
            {
                // User with that Username or Email already exist
                return (HttpStatusCode.Conflict, null);
            }

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                Role = Roles.User,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RefreshToken = String.Empty
            };

            var token = _jwtService.GenerateToken(user);
            await _userRepository.AddUserAsync(user);

            await _userProfileService.CreateProfileAsync(user.Id);

            return (HttpStatusCode.OK, token);
        }

        public async Task<(HttpStatusCode, string?, string?)> LoginUserAsync(LoginDto model)
        {
            var user = await _userRepository.GetUserByNameOrEmailAsync(model.UsernameOrEmail);
            if (user == null)
            {
                // Invalid Username or Email
                return (HttpStatusCode.NotFound, null, null);
            }

            if (user.IsBanned)
            {
                // User banned
                return (HttpStatusCode.Forbidden, null, null);
            }

            var verify = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
            if (!verify)
            {
                // Invalid Password
                return (HttpStatusCode.BadRequest, null, null);
            }

            var token = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(7);

            await _userRepository.UpdateUserAsync(user);

            _jwtService.SetRefreshTokenInCookie(refreshToken);

            return (HttpStatusCode.OK, token, refreshToken);
        }

        public async Task<(HttpStatusCode, string?, string?)> RefreshTokenAsync()
        {
            var refreshToken = _httpContextAccessor.HttpContext.Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return (HttpStatusCode.BadRequest, null, null);
            }

            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var principal = _jwtService.GetPrincipalFromExpiredToken(token);
            var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var userId = Guid.Parse(userIdClaim.Value);

            if (!await _jwtService.ValidateRefreshTokenAsync(userId, refreshToken))
            {
                return (HttpStatusCode.Unauthorized, null, null);
            }

            var user = await GetUserByIdAsync(userId);
            var newToken = _jwtService.GenerateToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateUserAsync(user);

            return (HttpStatusCode.OK, newToken, newRefreshToken);
        }

        public async Task<HttpStatusCode> UpdateUserByIdAsync(Guid id, UpdateUserDTO model, ClaimsPrincipal userClaim)
        {
            var currentUserId = GetCurrentUserId();
            if (id != currentUserId && !userClaim.IsInRole(Roles.Admin))
            {
                return HttpStatusCode.Forbidden;   // You are not allowed to update this user
            }

            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return HttpStatusCode.NotFound; // User not found
            }

            var existingUser = await _userRepository.GetUserByNameAsync(model.Username);
            if (existingUser != null && existingUser.Id != id)
            {
                return HttpStatusCode.Conflict; // Username already exists for another user
            }

            existingUser = await _userRepository.GetUserByEmailAsync(model.Email);
            if (existingUser != null && existingUser.Id != id)
            {
                return HttpStatusCode.Conflict; // Email already exists for another user
            }

            bool updated = false;

            if (!string.IsNullOrEmpty(model.Password))
            {
                var verify = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);

                if (verify)
                {
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

                    if (!string.IsNullOrEmpty(model.NewPassword))
                    {
                        user.PasswordHash = HashPassword(model.NewPassword);
                        updated = true;
                    }
                }
                else
                {
                    return HttpStatusCode.BadRequest; // Password is not correct
                }
            }

            if (updated)
            {
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
                return HttpStatusCode.OK;
            }

            return HttpStatusCode.NoContent;
        }

        public async Task<HttpStatusCode> UpdateUserRoleByIdAsync(Guid id, string role, ClaimsPrincipal userClaim)
        {
            if (!userClaim.IsInRole(Roles.Admin))
            {
                return HttpStatusCode.BadRequest;    // You are not allowed to set roles
            }

            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return HttpStatusCode.NotFound;     // User not found
            }

            if (!RoleValidator.IsValidRole(role))
            {
                return HttpStatusCode.BadRequest;   // Invalid role
            }

            user.Role = role;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _userRepository.UpdateUserAsync(user);

            return HttpStatusCode.OK;
        }

        public async Task<HttpStatusCode> DeleteUserByIdAsync(Guid id, ClaimsPrincipal userClaim)
        {
            var currentUserId = GetCurrentUserId();
            if (id != currentUserId && !userClaim.IsInRole(Roles.Admin))
            {
                return HttpStatusCode.BadRequest;   // You are not allowed to delete this user
            }

            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return HttpStatusCode.NotFound;     // User not found
            }

            await _userProfileService.DeleteUserAvatarByUserIdAsync(id, userClaim);
            await _userRepository.DeleteUserAsync(user);

            return HttpStatusCode.OK;
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

        public async Task<(HttpStatusCode, IEnumerable<UserDto>?, int)> GetAllUsersAsync(ClaimsPrincipal userClaim, int pageNumber, int pageSize)
        {
            if (!userClaim.IsInRole(Roles.Admin))
            {
                return (HttpStatusCode.BadRequest, null, 0);   // You are not allowed to get all users
            }

            var allUsers = await _userRepository.GetAllUsersAsync(pageNumber, pageSize);
            if (allUsers == null)
            {
                return (HttpStatusCode.NotFound, null, 0); // Users not found
            }

            var usersCount = await _userRepository.GetTotalCountUsersAsync();
            var totalPages = (int)Math.Ceiling(usersCount / (double)pageSize);

            var allUsersDto = allUsers.Select(user => new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                IsBanned = user.IsBanned
            });

            return (HttpStatusCode.OK, allUsersDto, totalPages);
        }

        public async Task<(HttpStatusCode, IEnumerable<UserDto>?, int)> SearchUsersAsync(string query, int pageNumber, int pageSize, ClaimsPrincipal userClaim)
        {
            if (!userClaim.IsInRole(Roles.Admin))
            {
                return (HttpStatusCode.BadRequest, null, 0);   // You are not allowed to get users
            }

            var queryableUsers = await _userRepository.GetUsersByQueryAsync(query);
            int usersCount = await queryableUsers.CountAsync();
            var totalPages = (int)Math.Ceiling(usersCount / (double)pageSize);

            var users = await queryableUsers
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!queryableUsers.Any())
            {
                return (HttpStatusCode.NotFound, null, totalPages); // Users not found
            }

            var usersDto = users.Select(user => new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                IsBanned = user.IsBanned
            });

            return (HttpStatusCode.OK, usersDto, totalPages);
        }

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<HttpStatusCode> BanUnbanUserByIdAsync(Guid id, bool isBanned, ClaimsPrincipal userClaim)
        {
            var currentUserId = GetCurrentUserId();
            if (id != currentUserId && !userClaim.IsInRole(Roles.Admin))
            {
                return HttpStatusCode.BadRequest;   // You are not allowed to Ban this user
            }

            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return HttpStatusCode.NotFound;     // User not found
            }

            user.IsBanned = isBanned;

            // Set all images private if user banned
            if (isBanned)
            {
                await _imageRepository.SetAllUserImagesPrivateAsync(id);
            }

            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateUserAsync(user);

            return HttpStatusCode.OK;
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}