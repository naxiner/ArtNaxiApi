
using ArtNaxiApi.Models;

namespace ArtNaxiApi.Repositories
{
    public interface IStyleRepository
    {
        Task<Style> GetStyleByNameAsync(string styleName);
    }
}