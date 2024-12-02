# ArtNaxi API

REST API for generating images using Stable Diffusion, with registration, authentication, user management, content management, and administration functionality.

## Features

- User registration
- User authentication with JWT
- Distribution of users by roles (User, Moderator, Admin)
- User profiles
- Image generation with a choice of parameters
- Apply styles to images
- Image likes
- Request caching with Redis
- Docker support for easy deployment

## Requirements

- [.NET 9.0 SDK (or later)](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Redis](https://hub.docker.com/_/redis)
- [Stable Diffusion](https://github.com/AUTOMATIC1111/stable-diffusion-webui) or [Stable Diffusion Forge](https://github.com/lllyasviel/stable-diffusion-webui-forge)

## Installing

**1. Clone repository:**
```bash
git clone https://github.com/naxiner/ArtNaxiApi.git
cd ArtNaxiApi/ArtNaxiApi
```

**2. Install dependencies:**
```bash
dotnet restore
```

**3. Setup connections:**

Configure the `appsettings.json` file.

**Databases:**
```json
"ConnectionStrings": {
  "ArtNaxiDbConnectionString": "Server=YourServer;Database=YourDbName;Integrated Security=True;Encrypt=True;TrustServerCertificate=True",
  "Redis": "YourServer"
}
```

**JWT:**
```json
"Jwt": {
  "Secret": "YourSecretKey(At least 256 bits)",
  "ExpiresHours": "12"
}
```

**Stable Diffusion:**
```
"StableDiffusion": {
  "ApiUrlTextToImg": "http://127.0.0.1:7860/sdapi/v1/txt2img",
}
```

**Frontend Settings:**
```
"FrontendSettings": {
  "AngularUrlHttps": "FrontendBaseUrl",
  "DefualtAvatarUrl": "UrlToDefaultAvatar"
}
```

**4. Start**
```
dotnet run --launch-profile https
```
API will be available at: 
- https://localhost:7069
- http://localhost:5256

[Frontend](https://github.com/naxiner/ArtNaxiFrontend/) will be available at:
- https://localhost:4200

At the first launch, a user with Admin role is automatically created.
Username: `admin`, Password: `Test123!`.

## Docker

**1. Clone repository:**
```bash
git clone https://github.com/naxiner/ArtNaxiApi.git
cd ArtNaxiApi
```

**Build container:**
```
docker-compose up --build
```

API will be available at: 
- https://localhost:8081
- http://localhost:8080

[Frontend](https://github.com/naxiner/ArtNaxiFrontend/) will be available at:
- https://localhost:4200

At the first launch, a user with Admin role is automatically created.
Username: `admin`, Password: `Test123!`.
