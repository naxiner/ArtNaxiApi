using ArtNaxiApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ArtNaxiApi.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Image> Images { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
    }
}
