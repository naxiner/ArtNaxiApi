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

        public async Task<Style> GetStyleByIdAsync(Guid id)
        {
            return await _context.Styles
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Style> GetStyleByNameAsync(string styleName)
        {
            return await _context.Styles
                .FirstOrDefaultAsync(s => s.Name == styleName);
        }

        public async Task<IEnumerable<Style>> GetAllStylesAsync(int pageNumber, int pageSize)
        {
            return await _context.Styles
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalStylesCountAsync()
        {
            return await _context.Styles.CountAsync();
        }

        public async Task AddStyleAsync(Style style)
        {
            _context.Styles.Add(style);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStyleAsync(Style style)
        {
            _context.Styles.Update(style);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteStyleByIdAsync(Guid id)
        {
            var style = await GetStyleByIdAsync(id);

            if (style == null)
            {
                throw new KeyNotFoundException();
            }

            _context.Styles.Remove(style);
            await _context.SaveChangesAsync();
        }
    }
}
