using ArtNaxiApi.Constants;
using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtNaxiApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult> RegisterUser(RegistrDto model)
        {
            var user = await _userService.RegisterUserAsync(model);

            if (user == null)
            {
                return BadRequest("User with that Username or Email already exist.");
            }

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult> LoginUser(LoginDto model)
        {
            var token = await _userService.LoginUserAsync(model);

            if (token == null)
            {
                return Unauthorized("Invalid Username or Password."); 
            }

            return Ok(token);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<User>> UpdateUser(Guid id, UpdateUserDTO model)
        {
            var currentUserId = _userService.GetCurrentUserId();
            var currentUser = await _userService.GetUserByIdAsync(currentUserId);

            if (id != currentUserId && !User.IsInRole(Roles.Admin))
            {
                return Forbid("You are not allowed to update this user.");
            }
            
            var updateUserResult = await _userService.UpdateUserByIdAsync(id, model);
            
            if (!updateUserResult)
            {
                return BadRequest("Failed to update user. Username or Email might be taken.");
            }

            return Ok("User updated successfully");
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            var currentUserId = _userService.GetCurrentUserId();
            var currentUser = await _userService.GetUserByIdAsync(currentUserId);

            if (id != currentUserId && !User.IsInRole(Roles.Admin))
            {
                return BadRequest("You are not allowed to delete this user.");
            }

            var result = await _userService.DeleteUserByIdAsync(id);
            
            if (!result)
            {
                return BadRequest("Can't delete a user with this Id.");
            }

            return Ok("User deleted successfully.");
        }
    }
}
