using ArtNaxiApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ArtNaxiApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>(
                options => options.UseSqlServer(builder.Configuration
                .GetConnectionString("ArtNaxiDbConnectionString"))
                );
            
            builder.Services.AddControllers();

            var app = builder.Build();

            app.MapGet("/", () => "Hello World!");

            app.MapControllers();

            app.Run();
        }
    }
}
