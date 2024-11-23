using ArtNaxiApi.Models.DTO.Responses;
using ArtNaxiApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ArtNaxiApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LikeController : ControllerBase
    {
        private readonly ILikeService _likeService;

        public LikeController(ILikeService likeService)
        {
            _likeService = likeService;
        }

        [HttpGet("{id}/count")]
        public async Task<ActionResult> GetLikeCountByEntityId(Guid id)
        {
            var (result, likeCount) = await _likeService.GetLikeCountByEntityIdAsync(id);

            return result switch
            {
                HttpStatusCode.OK => Ok(new CountResponse(likeCount)),
                _ => BadRequest()
            };
        }

        [Authorize]
        [HttpGet("like-status")]
        public async Task<ActionResult> GetLikeStatus(Guid userId, Guid entityId, string entityType)
        {
            var (result, isLiked) = await _likeService.GetLikeStatusAsync(userId, entityId, entityType);

            return result switch
            {
                HttpStatusCode.OK => Ok(new LikeStatusResponse(isLiked)),
                _ => BadRequest()
            };
        }

        [Authorize]
        [HttpPost("like")]
        public async Task<ActionResult> LikeEntity(Guid entityId, string entityType)
        {
            var result = await _likeService.LikeEntityAsync(entityId, entityType);

            return result switch
            {
                HttpStatusCode.OK => Ok(new MessageResponse("Liked successfully.")),
                HttpStatusCode.BadRequest => BadRequest(new MessageResponse("Invalid entity type.")),
                HttpStatusCode.Conflict => Conflict(new MessageResponse("Already liked.")),
                _ => BadRequest()
            };
        }

        [Authorize]
        [HttpPost("dislike")]
        public async Task<ActionResult> DislikeEntity(Guid entityId, string entityType)
        {
            var result = await _likeService.DislikeEntityAsync(entityId, entityType);

            return result switch
            {
                HttpStatusCode.OK => Ok(new MessageResponse("Disliked successfully.")),
                HttpStatusCode.BadRequest => BadRequest(new MessageResponse("Invalid entity type.")),
                HttpStatusCode.Conflict => Conflict(new MessageResponse("Like not exist.")),
                _ => BadRequest()
            };
        }
    }
}
