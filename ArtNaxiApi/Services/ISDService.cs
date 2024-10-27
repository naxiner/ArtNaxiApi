﻿using ArtNaxiApi.Models;
using System.Net;
using System.Security.Claims;

namespace ArtNaxiApi.Services
{
    public interface ISDService
    {
        Task<(HttpStatusCode, string?)> GenerateImageAsync(SDRequest request);
        Task<HttpStatusCode> DeleteImageByIdAsync(Guid id, ClaimsPrincipal user);
    }
}