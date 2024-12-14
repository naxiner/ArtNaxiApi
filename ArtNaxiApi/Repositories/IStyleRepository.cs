
using ArtNaxiApi.Models;

namespace ArtNaxiApi.Repositories
{
    public interface IStyleRepository
    {
        Task<Style?> GetStyleByIdAsync(Guid id);
        Task<Style?> GetStyleByNameAsync(string styleName);
        Task<IEnumerable<Style>?> GetAllStylesAsync(int pageNumber, int pageSize);
        Task<int> GetTotalStylesCountAsync();
        Task AddStyleAsync(Style style);
        Task UpdateStyleAsync(Style style);
        Task DeleteStyleByIdAsync(Guid id);
    }
}