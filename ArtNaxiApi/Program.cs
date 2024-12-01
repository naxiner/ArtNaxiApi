using ArtNaxiApi.Data;
using ArtNaxiApi.Filters;
using ArtNaxiApi.Repositories;
using ArtNaxiApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using StackExchange.Redis;
using ArtNaxiApi.Services.Cached;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography.X509Certificates;


namespace ArtNaxiApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ConfigureHttpsDefaults(httpsOptions =>
                {
                    httpsOptions.ServerCertificate = new X509Certificate2("certificates/artnaxiapi.pfx", "Test123!");
                });
            });

            builder.Services.AddStackExchangeRedisCache(redisOptions =>
            {
                string connection = builder.Configuration.GetConnectionString("Redis");

                redisOptions.Configuration = connection;
            });

            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var connectionString = builder.Configuration.GetConnectionString("Redis");
                return ConnectionMultiplexer.Connect(connectionString);
            });

            builder.Services.AddCors(options =>
            {
                string angularUrlHttps = builder.Configuration["FrontendSettings:AngularUrlHttps"]!;

                options.AddPolicy("Cors", builder =>
                {
                    builder
                        .WithOrigins(angularUrlHttps)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            builder.Services.AddDbContext<AppDbContext>(
                options => options.UseSqlServer(builder.Configuration
                .GetConnectionString("ArtNaxiDbConnectionString"))
                );

            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter token as Bearer {your token}",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });

            var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!));
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = jwtKey,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
            builder.Services.AddScoped<IImageRepository, ImageRepository>();
            builder.Services.AddScoped<IStyleRepository, StyleRepository>();
            builder.Services.AddScoped<ILikeRepository, LikeRepository>();

            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<ImageService>();
            builder.Services.AddScoped<IImageService>(provider =>
            {
                var baseService = provider.GetRequiredService<ImageService>();
                var redis = provider.GetRequiredService<IConnectionMultiplexer>();
                var cache = provider.GetRequiredService<IDistributedCache>();
                return new CachedImageService(cache, redis, baseService);
            });
            builder.Services.AddScoped<ISDService, SDService>();
            builder.Services.AddScoped<IStyleService, StyleService>();
            builder.Services.AddScoped<ILikeService, LikeService>();

            builder.Services.AddScoped<CheckBanAttribute>();

            var app = builder.Build();

            DbInitializer.InitializeAsync(app.Services).GetAwaiter().GetResult();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseCors("Cors");

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
