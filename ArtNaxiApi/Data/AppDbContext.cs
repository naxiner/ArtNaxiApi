using ArtNaxiApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ArtNaxiApi.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Image> Images { get; set; }
        public DbSet<SDRequest> SDRequests { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Image>()
                .HasOne(i => i.Request)
                .WithOne(r => r.Image)
                .HasForeignKey<SDRequest>(r => r.ImageId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
