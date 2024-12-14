using ArtNaxiApi.Constants;
using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Repositories;
using System.Net;
using System.Security.Claims;

namespace ArtNaxiApi.Services
{
    public class StyleService : IStyleService
    {
        private readonly IStyleRepository _styleRepository;

        public StyleService(IStyleRepository styleRepository)
        {
            _styleRepository = styleRepository;
        }

        public async Task<(HttpStatusCode, StyleDto?)> GetStyleByIdAsync(Guid id)
        {
            var style = await _styleRepository.GetStyleByIdAsync(id);
            if (style == null)
            {
                return (HttpStatusCode.NotFound, null);     // Style not found
            }

            var styleDto = new StyleDto 
            {
                Id = style.Id,
                Name = style.Name
            };

            return (HttpStatusCode.OK, styleDto);
        }

        public async Task<(HttpStatusCode, StyleDto?)> GetStyleByNameAsync(string styleName)
        {
            var style = await _styleRepository.GetStyleByNameAsync(styleName);
            if (style == null)
            {
                return (HttpStatusCode.NotFound, null);     // Style not found
            }

            var styleDto = new StyleDto
            {
                Id = style.Id,
                Name = style.Name
            };

            return (HttpStatusCode.OK, styleDto);
        }

        public async Task<(HttpStatusCode, IEnumerable<StyleDto>?, int)> GetAllStylesAsync(int pageNumber, int pageSize)
        {
            var styles = await _styleRepository.GetAllStylesAsync(pageNumber, pageSize);
            if (styles == null)
            {
                return (HttpStatusCode.NotFound, null, 0);     // Styles not found
            }

            var stylesCount = await _styleRepository.GetTotalStylesCountAsync();
            var totalPages = CountTotalPages(stylesCount, pageSize);

            var stylesDto = styles.Select(style => new StyleDto
            {
                Id = style.Id,
                Name = style.Name
            });

            return (HttpStatusCode.OK, stylesDto, totalPages);
        }

        public async Task<(HttpStatusCode, int)> GetTotalStylesCountAsync()
        {
            var stylesCount = await _styleRepository.GetTotalStylesCountAsync();

            return (HttpStatusCode.OK, stylesCount);
        }

        public async Task<HttpStatusCode> AddStyleAsync(AddStyleDto addStyleDto, ClaimsPrincipal userClaim)
        {
            if (!userClaim.IsInRole(Roles.Admin))
            {
                return HttpStatusCode.BadRequest;    // You are not allowed to add style
            }

            if (await _styleRepository.GetStyleByNameAsync(addStyleDto.Name) != null)
            {
                return HttpStatusCode.Conflict;     // Style with that name already exist
            }

            var style = new Style
            {
                Id = Guid.NewGuid(),
                Name = addStyleDto.Name
            };

            await _styleRepository.AddStyleAsync(style);
            return HttpStatusCode.OK;
        }

        public async Task<HttpStatusCode> UpdateStyleByIdAsync(Guid id, StyleDto styleDto, ClaimsPrincipal userClaim)
        {
            if (!userClaim.IsInRole(Roles.Admin))
            {
                return HttpStatusCode.BadRequest;   // You are not allowed to update style
            }

            var style = await _styleRepository.GetStyleByIdAsync(id);
            if (style == null)
            {
                return HttpStatusCode.NotFound;     // Style not found
            }

            if (!string.IsNullOrEmpty(style.Name))
            {
                style.Name = styleDto.Name;
                await _styleRepository.UpdateStyleAsync(style);
                return HttpStatusCode.OK;
            }

            return HttpStatusCode.NoContent;
        }

        public async Task<HttpStatusCode> DeleteStyleByIdAsync(Guid id, ClaimsPrincipal userClaim)
        {
            var style = await _styleRepository.GetStyleByIdAsync(id);
            if (style == null)
            {
                return HttpStatusCode.NotFound;     // Style not found
            }

            await _styleRepository.DeleteStyleByIdAsync(id);
            return HttpStatusCode.NoContent;
        }

        private int CountTotalPages(int itemCount, int pageSize)
        {
            return (int)Math.Ceiling(itemCount / (double)pageSize);
        }
    }
}
