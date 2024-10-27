﻿using ArtNaxiApi.Models;
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
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult> RegisterUser(RegistrDto model)
        {
            var result = await _userService.RegisterUserAsync(model);

            return result switch
            {
                HttpStatusCode.Conflict => Conflict("User with that Username or Email already exist."),
                HttpStatusCode.OK => Ok("User register successfull."),
                _ => BadRequest()
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult> LoginUser(LoginDto model)
        {
            var (result, token) = await _userService.LoginUserAsync(model);

            return result switch
            {
                HttpStatusCode.NotFound => NotFound("Invalid Username or Email."),
                HttpStatusCode.BadRequest => Forbid("Invalid Password."),
                HttpStatusCode.OK => Ok(token),
                _ => BadRequest()
            };
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<User>> UpdateUser(Guid id, UpdateUserDTO model)
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
