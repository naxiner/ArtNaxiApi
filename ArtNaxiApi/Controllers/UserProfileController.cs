using ArtNaxiApi.Models;
using ArtNaxiApi.Services;
using Microsoft.AspNetCore.Mvc;

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
            var userProfile = await _userProfileService.GetProfileByUserIdAsync(id);

            if (userProfile == null)
            {
                return NotFound();
            }

            var userProfileDto = _userProfileService.MapToUserProfileDto(userProfile);

            return Ok(userProfileDto);
        }
    }
}
