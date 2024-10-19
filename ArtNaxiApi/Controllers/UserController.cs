using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Services;
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
    }
}
