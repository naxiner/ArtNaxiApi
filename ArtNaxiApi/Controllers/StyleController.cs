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
    public class StyleController : ControllerBase
    {
        private readonly IStyleService _styleService;

        public StyleController(IStyleService styleService)
        {
            _styleService = styleService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetStyleByIdAsync(Guid id)
        {
            var (result, style) = await _styleService.GetStyleByIdAsync(id);
            
            return result switch
            {
                HttpStatusCode.OK => Ok(new StyleResponse(style)),
                HttpStatusCode.NotFound => NotFound(new MessageResponse("Style not found.")),
                _ => BadRequest()
            };
        }

        [HttpGet("name/{styleName}")]
        public async Task<ActionResult> GetStyleByNameAsync(string styleName)
        {
            var (result, style) = await _styleService.GetStyleByNameAsync(styleName);
            
            return result switch
            {
                HttpStatusCode.OK => Ok(new StyleResponse(style)),
                HttpStatusCode.NotFound => NotFound(new MessageResponse("Style not found.")),
                _ => BadRequest()
            };
        }

        [HttpGet]
        public async Task<ActionResult> GetAllStylesAsync(int pageNumber = 1, int pageSize = 10)
        {
            var (result, styles, totalPages) = await _styleService.GetAllStylesAsync(pageNumber, pageSize);
            
            return result switch
            {
                HttpStatusCode.OK => Ok(new StylesResponse(styles, totalPages)),
                HttpStatusCode.NotFound => NotFound(new MessageResponse("Styles not found.")),
                _ => BadRequest()
            };
        }

        [HttpGet("total-count")]
        public async Task<ActionResult> GetTotalStylesCountAsync()
        {
            var (result, totalCount) = await _styleService.GetTotalStylesCountAsync();

            return result switch
            {
                HttpStatusCode.OK => Ok(new CountResponse(totalCount)),
                HttpStatusCode.NotFound => NotFound(new MessageResponse("Styles not found.")),
                _ => BadRequest()
            };
        }

        [Authorize]
        [ServiceFilter(typeof(CheckBanAttribute))]
        [HttpPost]
        public async Task<ActionResult> AddStyleAsync(AddStyleDto addStyleDto)
        {
            var result = await _styleService.AddStyleAsync(addStyleDto, User);

            return result switch
            {
                HttpStatusCode.OK => Ok(new MessageResponse("Style added successfully.")),
                HttpStatusCode.BadRequest => BadRequest(new MessageResponse("You are not allowed to add style.")),
                HttpStatusCode.Conflict => Conflict(new MessageResponse("Style with that name already exist.")),
                _ => BadRequest()
            };
        }

        [Authorize]
        [ServiceFilter(typeof(CheckBanAttribute))]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStyleByIdAsync(Guid id)
        {
            var result = await _styleService.DeleteStyleByIdAsync(id, User);

            return result switch
            {
                HttpStatusCode.NoContent => NoContent(),
                HttpStatusCode.NotFound => NotFound(new MessageResponse("Style not found.")),
                _ => BadRequest()
            };
        }
    }
}
