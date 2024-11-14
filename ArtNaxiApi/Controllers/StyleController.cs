using ArtNaxiApi.Services;
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
                HttpStatusCode.NotFound => NotFound(new { message = "Style not found." }),
                HttpStatusCode.OK => Ok(new { message = "Style received successfully.", style }),
                _ => BadRequest()
            };
        }

        [HttpGet("name/{styleName}")]
        public async Task<ActionResult> GetStyleByNameAsync(string styleName)
        {
            var (result, style) = await _styleService.GetStyleByNameAsync(styleName);
            
            return result switch
            {
                HttpStatusCode.NotFound => NotFound(new { message = "Style not found." }),
                HttpStatusCode.OK => Ok(new { message = "Style received successfully.", style }),
                _ => BadRequest()
            };
        }

        [HttpGet]
        public async Task<ActionResult> GetAllStylesAsync(int pageNumber = 1, int pageSize = 10)
        {
            var (result, styles, totalPages) = await _styleService.GetAllStylesAsync(pageNumber, pageSize);
            
            return result switch
            {
                HttpStatusCode.NotFound => NotFound(new { message = "Styles not found.", totalPages }),
                HttpStatusCode.OK => Ok(new { message = "All styles received successfully.", styles, totalPages }),
                _ => BadRequest()
            };
        }

        [HttpGet("total-count")]
        public async Task<ActionResult> GetTotalStylesCountAsync()
        {
            var (result, totalCount) = await _styleService.GetTotalStylesCountAsync();

            return result switch
            {
                HttpStatusCode.NotFound => NotFound(new { message = "Styles not found.", totalCount }),
                HttpStatusCode.OK => Ok(new { totalCount }),
                _ => BadRequest()
            };
        }
    }
}
