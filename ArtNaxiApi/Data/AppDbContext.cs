using ArtNaxiApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ArtNaxiApi.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<SDRequest> SDRequests { get; set; }
        public DbSet<Style> Styles { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Image>()
                .HasOne(i => i.Request)
                .WithOne(r => r.Image)
                .HasForeignKey<SDRequest>(r => r.ImageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SDRequestStyle>()
                .HasKey(rs => new { rs.SDRequestId, rs.StyleId });

            modelBuilder.Entity<SDRequestStyle>()
                .HasOne(rs => rs.SDRequest)
                .WithMany(r => r.SDRequestStyles)
                .HasForeignKey(rs => rs.SDRequestId);

            modelBuilder.Entity<SDRequestStyle>()
                .HasOne(rs => rs.Style)
                .WithMany(s => s.SDRequestStyles)
                .HasForeignKey(rs => rs.StyleId);
        }
    }
}
