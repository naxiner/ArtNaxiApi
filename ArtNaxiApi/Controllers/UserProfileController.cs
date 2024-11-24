using ArtNaxiApi.Models.DTO.Responses;
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
                HttpStatusCode.OK => Ok(new UserProfileResponse(userProfileDto)),
                HttpStatusCode.NotFound => NotFound(new MessageResponse("User with this Id not found.")),
                _ => BadRequest()
            };
        }

        [HttpGet("avatar/{id}")]
        public async Task<ActionResult> GetUserAvatarByIdAsync(Guid id)
        {
            var (result, userAvatarUrl) = await _userProfileService.GetProfileAvatarByUserIdAsync(id);

            return result switch
            {
                HttpStatusCode.OK => Ok(new AvatarResponse(String.Empty, userAvatarUrl)),
                HttpStatusCode.NotFound => NotFound(new AvatarResponse("Avatar not found.", userAvatarUrl)),
                _ => BadRequest()
            };
        }

        [Authorize]
        [HttpPut("avatar/{id}")]
        public async Task<ActionResult> UpdateProfileAvatarById(Guid id, IFormFile avatarFile)
        {
            var (result, userAvatarUrl) = await _userProfileService.UpdateProfileAvatarByUserIdAsync(id, avatarFile);

            return result switch
            {
                HttpStatusCode.OK => Ok(new AvatarResponse("Avatar updated successful.", userAvatarUrl)),
                HttpStatusCode.Forbidden => Forbid(),
                HttpStatusCode.BadRequest => BadRequest(new MessageResponse("No file uploaded.")),
                _ => BadRequest()
            };
        }

        [Authorize]
        [HttpDelete("avatar/{id}")]
        public async Task<ActionResult> DeleteUserAvatarById(Guid id)
        {
            var result = await _userProfileService.DeleteUserAvatarByUserIdAsync(id, User);

            return result switch
            {
                HttpStatusCode.OK => Ok(new MessageResponse("Avatar deleted successfully.")),
                HttpStatusCode.BadRequest => BadRequest(new MessageResponse("You are not allowed to delete this avatar.")),
                _ => BadRequest()
            };
        }

        [Authorize]
        [HttpGet("{id}/all-image-count")]
        public async Task<ActionResult> GetAllImageCountById(Guid id)
        {
            var allImageCount = await _userProfileService.GetAllImageCountByUserIdAsync(id);

            return Ok(new CountResponse(allImageCount));
        }

        [HttpGet("{id}/public-image-count")]
        public async Task <ActionResult> GetPublicImageCountById(Guid id)
        {
            var publicImageCount = await _userProfileService.GetPublicImageCountByUserIdAsync(id);

            return Ok(new CountResponse(publicImageCount));
        }
    }
}
