using ArtNaxiApi.Filters;
using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Models.DTO.Responses;
using ArtNaxiApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ArtNaxiApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        public UserController(IUserService userService, IJwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<ActionResult> RegisterUser(RegistrDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (result, token) = await _userService.RegisterUserAsync(model);

            return result switch
            {
                HttpStatusCode.OK => Ok(new RegisterResponse("User register successful.", token)),
                HttpStatusCode.Conflict => Conflict(new MessageResponse("User with that Username or Email already exist.")),
                _ => BadRequest()
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult> LoginUser(LoginDto model)
        {
            var (result, token, refreshToken) = await _userService.LoginUserAsync(model);

            return result switch
            {
                HttpStatusCode.OK => Ok(new LoginResponse(token, refreshToken)),
                HttpStatusCode.NotFound => NotFound(new MessageResponse("Invalid Username or Email.")),
                HttpStatusCode.BadRequest => BadRequest(new MessageResponse("Invalid Password.")),
                HttpStatusCode.Forbidden => Forbid(),
                _ => BadRequest()
            };
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            _jwtService.RemoveRefreshTokenFromCookie();

            return Ok();
        }

        [Authorize]
        [HttpPost("refresh-token")]
        public async Task<ActionResult> RefreshToken()
        {
            var (result, newToken, newRefreshToken) = await _userService.RefreshTokenAsync();

            return result switch
            {
                HttpStatusCode.OK => Ok(new LoginResponse(newToken, newRefreshToken)),
                HttpStatusCode.Unauthorized => Unauthorized(new MessageResponse("Invalid refresh token.")),
                HttpStatusCode.BadRequest => BadRequest(new MessageResponse("Refresh token is missing.")),
                _ => BadRequest()
            };
        }

        [Authorize]
        [ServiceFilter(typeof(CheckBanAttribute))]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUser(Guid id, UpdateUserDTO model)
        {
            var result = await _userService.UpdateUserByIdAsync(id, model, User);

            return result switch
            {
                HttpStatusCode.OK => Ok(new MessageResponse("User updated successfully.")),
                HttpStatusCode.NotFound => NotFound(new MessageResponse("User not found.")),
                HttpStatusCode.Conflict => Conflict(new MessageResponse("Username or email already exist for another user.")),
                HttpStatusCode.BadRequest => BadRequest(new MessageResponse("Invalid password.")),
                HttpStatusCode.Forbidden => Forbid(),
                HttpStatusCode.NoContent => NoContent(),
                _ => BadRequest()
            };
        }

        [Authorize]
        [ServiceFilter(typeof(CheckBanAttribute))]
        [HttpPost("{id}/role")]
        public async Task<ActionResult> UpdateUserRole(Guid id, string role)
        {
            var result = await _userService.UpdateUserRoleByIdAsync(id, role, User, false);

            return result switch
            {
                HttpStatusCode.OK => Ok(new MessageResponse("Role has been successfully assigned.")),
                HttpStatusCode.BadRequest => BadRequest(new MessageResponse("You are not allowed to set this role.")),
                HttpStatusCode.NotFound => NotFound(new MessageResponse("User not found.")),
                _ => BadRequest()
            };
        }

        [Authorize]
        [ServiceFilter(typeof(CheckBanAttribute))]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            var result = await _userService.DeleteUserByIdAsync(id, User);

            return result switch
            {
                HttpStatusCode.OK => Ok(new MessageResponse("User deleted successfully.")),
                HttpStatusCode.BadRequest => BadRequest(new MessageResponse("You are not allowed to delete this user.")),
                HttpStatusCode.NotFound => NotFound(new MessageResponse("User not found.")),
                _ => BadRequest()
            };
        }

        [Authorize]
        [ServiceFilter(typeof(CheckBanAttribute))]
        [HttpGet]
        public async Task<ActionResult> GetAllUsers(int pageNumber = 1, int pageSize = 10)
        {
            var (result, users, totalPages) = await _userService.GetAllUsersAsync(User, pageNumber, pageSize);

            return result switch
            {
                HttpStatusCode.OK => Ok(new UsersResponse(users, totalPages)),
                HttpStatusCode.BadRequest => BadRequest(new MessageResponse("You are not allowed to get all users.")),
                HttpStatusCode.NotFound => NotFound(new MessageResponse("Users not found.")),
                _ => BadRequest()
            };
        }

        [Authorize]
        [ServiceFilter(typeof(CheckBanAttribute))]
        [HttpGet("search")]
        public async Task<ActionResult> GetUsersByQuery(string query, int pageNumber = 1, int pageSize = 10)
        {
            var (result, users, totalPages) = await _userService.SearchUsersAsync(query, pageNumber, pageSize, User);

            return result switch
            {
                HttpStatusCode.OK => Ok(new UsersResponse(users, totalPages)),
                HttpStatusCode.BadRequest => BadRequest(new MessageResponse("You are not allowed to search users.")),
                HttpStatusCode.NotFound => NotFound(new MessageResponse("Users not found.")),
                _ => BadRequest()
            };
        }

        [Authorize]
        [ServiceFilter(typeof(CheckBanAttribute))]
        [HttpPost("{id}/ban")]
        public async Task<ActionResult> BanUser(Guid id)
        {
            var result = await _userService.BanUnbanUserByIdAsync(id, true, User);

            return result switch
            {
                HttpStatusCode.OK => Ok(new MessageResponse("User banned successfully.")),
                HttpStatusCode.BadRequest => BadRequest(new MessageResponse("You are not allowed to Ban this user.")),
                HttpStatusCode.NotFound => NotFound(new MessageResponse("User not found.")),
                _ => BadRequest()
            };
        }

        [Authorize]
        [ServiceFilter(typeof(CheckBanAttribute))]
        [HttpPost("{id}/unban")]
        public async Task<ActionResult> UnbanUser(Guid id)
        {
            var result = await _userService.BanUnbanUserByIdAsync(id, false, User);

            return result switch
            {
                HttpStatusCode.OK => Ok(new MessageResponse("User unbanned successfully.")),
                HttpStatusCode.BadRequest => BadRequest(new MessageResponse("You are not allowed to Unban this user.")),
                HttpStatusCode.NotFound => NotFound(new MessageResponse("User not found.")),
                _ => BadRequest()
            };
        }
    }
}
