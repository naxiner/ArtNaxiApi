using ArtNaxiApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ArtNaxiApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        public UserProfileController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetUserProfileAsync(Guid id)
        {
            var (result, userProfileDto) = await _userProfileService.GetProfileByUserIdAsync(id);

            return result switch
            {
                HttpStatusCode.NotFound => NotFound("User with this Id not found."),
                HttpStatusCode.OK => Ok(userProfileDto),
                _ => BadRequest()
            };
        }
    }
}
