FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app

RUN mkdir -p /app/wwwroot/avatars \
    && chown -R app:app /app/wwwroot/avatars && chmod -R 755 /app/wwwroot/avatars
RUN mkdir -p /app/wwwroot/Images \
    && chown -R app:app /app/wwwroot/Images && chmod -R 755 /app/wwwroot/Images

EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ArtNaxiApi.csproj", "."]
RUN dotnet restore "ArtNaxiApi.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "ArtNaxiApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ArtNaxiApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArtNaxiApi.dll"]