using ArtNaxiApi.Services;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("avatar/{id}")]
        public async Task<ActionResult> GetUserAvatarByIdAsync(Guid id)
        {
            var (result, userAvatarUrl) = await _userProfileService.GetProfileAvatarByUserIdAsync(id);

            return result switch
            {
                HttpStatusCode.NotFound => NotFound(new { message = "Avatar not found.", userAvatarUrl }),
                HttpStatusCode.OK => Ok(new { userAvatarUrl }),
                _ => BadRequest()
            };
        }

        [Authorize]
        [HttpPut("avatar/{id}")]
        public async Task<ActionResult> UpdateProfileAvatarById(Guid id, IFormFile avatarFile)
        {
            var (result, profilePictureUrl) = await _userProfileService.UpdateProfileAvatarByUserIdAsync(id, avatarFile);

            return result switch
            {
                HttpStatusCode.Forbidden => Forbid(),
                HttpStatusCode.BadRequest => BadRequest(new { message = "No file uploaded" }),
                HttpStatusCode.OK => Ok(new { message = "Avatar updated successful.", profilePictureUrl }),
                _ => BadRequest()
            };
        }

        [HttpGet("{id}/public-image-count")]
        public async Task <ActionResult> GetPublicImageCountById(Guid id)
        {
            var publicImageCount = await _userProfileService.GetPublicImageCountByUserIdAsync(id);

            return Ok(new { publicImageCount });
        }
    }
}
