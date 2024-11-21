using ArtNaxiApi.Constants;
using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Services;
using Microsoft.EntityFrameworkCore;

namespace ArtNaxiApi.Data
{
    public class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            context.Database.Migrate();
            await AddInitialAdminAsync(context, userService);
        }

        private static async Task AddInitialAdminAsync(AppDbContext context, IUserService userService)
        {
            if (!context.Users.Any(u => u.Username == "admin"))
            {
                var username = "admin";
                var email = "admin@artnaxi.com";
                var password = "Test123!";

                var (status, token) = await userService.RegisterUserAsync(new RegistrDto
                {
                    Username = username,
                    Email = email,
                    Password = BCrypt.Net.BCrypt.HashPassword(password)
                });

                var adminUser = await userService.GetUserByNameAsync(username);
                if (adminUser != null)
                {
                    var updateStatus = await userService.UpdateUserRoleByIdAsync(adminUser.Id, Roles.Admin, new System.Security.Claims.ClaimsPrincipal());
                }
            }
        }
    }
}
