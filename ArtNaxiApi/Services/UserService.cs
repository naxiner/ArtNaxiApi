﻿using ArtNaxiApi.Constants;
using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Repositories;
using System.Net;
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

        public async Task<HttpStatusCode> RegisterUserAsync(RegistrDto model)
        {
            if (await _userRepository.GetUserByNameAsync(model.Username) != null ||
                await _userRepository.GetUserByEmailAsync(model.Email) != null)
            {
                // User with that Username or Email already exist
                return HttpStatusCode.Conflict;
            }

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                Role = Roles.User,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddUserAsync(user);

            await _userProfileService.CreateProfileAsync(user.Id);

            return HttpStatusCode.OK;
        }

        public async Task<(HttpStatusCode, string?)> LoginUserAsync(LoginDto model)
        {
            var user = await _userRepository.GetUserByNameOrEmailAsync(model.UsernameOrEmail);
            if (user == null)
            {
                // Invalid Username or Email
                return (HttpStatusCode.NotFound, null);
            }

            var verify = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
            if (!verify)
            {
                // Invalid Password
                return (HttpStatusCode.BadRequest, null);
            }

            var token = _jwtService.GenerateToken(user);
            return (HttpStatusCode.OK, token);
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
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
                return HttpStatusCode.OK;
            }

            return HttpStatusCode.NoContent;
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
                return HttpStatusCode.NotFound; // User not found
            }

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

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}