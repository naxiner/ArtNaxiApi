using ArtNaxiApi.Data;
using ArtNaxiApi.Services;
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

            builder.Services.AddHttpClient();
            builder.Services.AddControllers();

            builder.Services.AddScoped<ISDService, SDService>();

            var app = builder.Build();

            app.MapGet("/", () => "Hello World!");

            app.MapControllers();

            app.Run();
        }
    }
}
