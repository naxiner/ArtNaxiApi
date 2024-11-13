using ArtNaxiApi.Data;
using ArtNaxiApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ArtNaxiApi.Repositories
{
    public class StyleRepository : IStyleRepository
    {
        private AppDbContext _context;

        public StyleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Style> GetStyleByNameAsync(string styleName)
        {
            var stylesByName = await _context.Styles
                .FirstOrDefaultAsync(s => s.Name == styleName);

            return stylesByName;
        }
    }
}
