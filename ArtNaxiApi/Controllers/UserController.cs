using ArtNaxiApi.Models.DTO;
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

            var result = await _userService.RegisterUserAsync(model);

            return result switch
            {
                HttpStatusCode.Conflict => Conflict("User with that Username or Email already exist."),
                HttpStatusCode.OK => Ok("User register successful."),
                _ => BadRequest()
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult> LoginUser(LoginDto model)
        {
            var (result, token, refreshToken) = await _userService.LoginUserAsync(model);

            return result switch
            {
                HttpStatusCode.NotFound => NotFound("Invalid Username or Email."),
                HttpStatusCode.BadRequest => BadRequest("Invalid Password."),
                HttpStatusCode.OK => Ok(new { token, refreshToken }),
                _ => BadRequest()
            };
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            _jwtService.RemoveRefreshTokenFromCookie();

            return Ok();
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult> RefreshToken()
        {
            var (result, newToken, newRefreshToken) = await _userService.RefreshTokenAsync();

            return result switch
            {
                HttpStatusCode.Unauthorized => Unauthorized("Invalid refresh token."),
                HttpStatusCode.BadRequest => Unauthorized("Refresh token is missing."),
                HttpStatusCode.OK => Ok(new { token = newToken, refreshToken = newRefreshToken }),
                _ => BadRequest()
            };
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUser(Guid id, UpdateUserDTO model)
        {
            var result = await _userService.UpdateUserByIdAsync(id, model, User);

            return result switch
            {
                HttpStatusCode.NotFound => NotFound("User not found."),
                HttpStatusCode.Conflict => Conflict("Username or email already exist for another user."),
                HttpStatusCode.Forbidden => Forbid(),
                HttpStatusCode.NoContent => NoContent(),
                HttpStatusCode.OK => Ok("User updated successfully."),
                _ => BadRequest()
            };
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            var result = await _userService.DeleteUserByIdAsync(id, User);

            return result switch
            {
                HttpStatusCode.BadRequest => BadRequest("You are not allowed to delete this user."),
                HttpStatusCode.NotFound => NotFound("User not found."),
                HttpStatusCode.OK => Ok("User deleted successfully."),
                _ => BadRequest()
            };
        }
    }
}
