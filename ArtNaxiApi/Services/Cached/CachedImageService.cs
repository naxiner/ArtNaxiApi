using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Net;
using System.Security.Claims;

namespace ArtNaxiApi.Services.Cached
{
    public class CachedImageService : IImageService
    {
        private readonly IImageService _baseService;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDistributedCache _distributedCache;

        public CachedImageService(
            IDistributedCache cache, 
            IConnectionMultiplexer redis, 
            IImageService imageService)
        {
            _distributedCache = cache;
            _redis = redis;
            _baseService = imageService;
        }

        public async Task<(HttpStatusCode, IEnumerable<ImageDto>, int)> GetAllImagesAsync(int pageNumber, int pageSize, ClaimsPrincipal userClaim)
        {
            return await _baseService.GetAllImagesAsync(pageNumber, pageSize, userClaim);
        }

        public async Task<(HttpStatusCode, ImageDto?)> GetImageByIdAsync(Guid id)
        {
            string key = $"image-by-id_{id}";
            var cachedData = await _distributedCache.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonConvert.DeserializeObject<(HttpStatusCode, ImageDto?)>(cachedData)!;
            }

            var data = await _baseService.GetImageByIdAsync(id);

            if (data.Item1 == HttpStatusCode.OK)
            {
                await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(data), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
            }

            return data;
        }

        public async Task<(HttpStatusCode, IEnumerable<ImageDto>, int)> GetImagesByUserIdAsync(Guid userId, int pageNumber, int pageSize, ClaimsPrincipal userClaim)
        {
            string key = $"images-by-userid_{userId}_{pageNumber}_{pageSize}";
            var database = _redis.GetDatabase();
            var cachedData = await _distributedCache.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonConvert.DeserializeObject<(HttpStatusCode, IEnumerable<ImageDto>, int)>(cachedData)!;
            }

            var data = await _baseService.GetImagesByUserIdAsync(userId, pageNumber, pageSize, userClaim);

            if (data.Item1 == HttpStatusCode.OK)
            {
                await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(data), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

                string userSetKey = $"cache-keys-by-userid_{userId}";
                await database.SetAddAsync(userSetKey, key);
            }

            return data;
        }

        public async Task<(HttpStatusCode, IEnumerable<ImageDto>, int)> GetPopularPublicImagesAsync(int pageNumber, int pageSize)
        {
            string key = $"popular-public-images_{pageNumber}_{pageSize}";
            var cachedData = await _distributedCache.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonConvert.DeserializeObject<(HttpStatusCode, IEnumerable<ImageDto>, int)>(cachedData)!;
            }

            var data = await _baseService.GetPopularPublicImagesAsync(pageNumber, pageSize);

            if (data.Item1 == HttpStatusCode.OK)
            {
                await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(data), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
            }

            return data;
        }

        public async Task<(HttpStatusCode, IEnumerable<ImageDto>, int)> GetPublicImagesByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            string key = $"public-images-by-userid_{userId}_{pageNumber}_{pageSize}";
            var database = _redis.GetDatabase();
            var cachedData = await _distributedCache.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonConvert.DeserializeObject<(HttpStatusCode, IEnumerable<ImageDto>, int)>(cachedData)!;
            }

            var data = await _baseService.GetPublicImagesByUserIdAsync(userId, pageNumber, pageSize);

            if (data.Item1 == HttpStatusCode.OK)
            {
                await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(data), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

                string userSetKey = $"cache-keys-by-userid_{userId}";
                await database.SetAddAsync(userSetKey, key);
            }

            return data;
        }

        public async Task<(HttpStatusCode, IEnumerable<ImageDto>, int)> GetRecentImagesAsync(int pageNumber, int pageSize)
        {
            string key = $"recent-images_{pageNumber}_{pageSize}";
            var cachedData = await _distributedCache.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonConvert.DeserializeObject<(HttpStatusCode, IEnumerable<ImageDto>, int)>(cachedData)!;
            }

            var data = await _baseService.GetRecentImagesAsync(pageNumber, pageSize);

            if (data.Item1 == HttpStatusCode.OK)
            {
                await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(data), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
            }

            return data;
        }

        public async Task<(HttpStatusCode, IEnumerable<ImageDto>, int)> GetRecentPublicImagesAsync(int pageNumber, int pageSize)
        {
            string key = $"recent-public-images_{pageNumber}_{pageSize}";
            var cachedData = await _distributedCache.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonConvert.DeserializeObject<(HttpStatusCode, IEnumerable<ImageDto>, int)>(cachedData)!;
            }

            var data = await _baseService.GetRecentPublicImagesAsync(pageNumber, pageSize);

            if (data.Item1 == HttpStatusCode.OK)
            {
                await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(data), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
            }

            return data;
        }

        public async Task<HttpStatusCode> MakeImagePrivateAsync(Guid id)
        {
            var image = await _baseService.GetImageByIdAsync(id);
            var result = await _baseService.MakeImagePrivateAsync(id);

            if (result == HttpStatusCode.OK)
            {
                await InvalidateCacheForUserAsync(image.Item2.UserId, id);
                await InvalidatePublicCache();
            }

            return result;
        }

        public async Task<HttpStatusCode> MakeImagePublicAsync(Guid id)
        {
            var image = await _baseService.GetImageByIdAsync(id);
            var result = await _baseService.MakeImagePublicAsync(id);

            if (result == HttpStatusCode.OK)
            {
                await InvalidateCacheForUserAsync(image.Item2.UserId, id);
                await InvalidatePublicCache();
            }

            return result;
        }

        public async Task<HttpStatusCode> DeleteImageByIdAsync(Guid id, ClaimsPrincipal userClaim)
        {
            var image = await _baseService.GetImageByIdAsync(id);
            var result = await _baseService.DeleteImageByIdAsync(id, userClaim);
            
            if (result == HttpStatusCode.NoContent)
            {
                await InvalidateCacheForUserAsync(image.Item2.UserId, id);

                if (image.Item2.IsPublic)
                {
                    await InvalidatePublicCache();
                }
            }

            return result;
        }

        public async Task<(HttpStatusCode, ImageDto?)> GenerateImageAsync(SDRequest sdRequest)
        {
            var result = await _baseService.GenerateImageAsync(sdRequest);

            if (result.Item1 == HttpStatusCode.OK)
            {
                await InvalidateCacheForUserAsync(result.Item2.UserId, result.Item2.Id);
            }

            return result;
        }

        private async Task InvalidateCacheForUserAsync(Guid? userId, Guid? imageId)
        {
            var database = _redis.GetDatabase();

            if (userId != null)
            {
                // Видаляємо кеш ключів для користувача
                string userSetKey = $"cache-keys-by-userid_{userId}";
                var userKeys = await database.SetMembersAsync(userSetKey);

                foreach (var key in userKeys)
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        await database.KeyDeleteAsync((RedisKey)key.ToString());
                    }
                }

                await database.KeyDeleteAsync(userSetKey);
            }

            if (imageId != null)
            {
                string imageKey = $"image-by-id_{imageId}";
                await database.KeyDeleteAsync(imageKey);
            }
        }

        private async Task InvalidatePublicCache()
        {
            var database = _redis.GetDatabase();

            for (int pageNumber = 1; pageNumber <= 5; pageNumber++)
            {
                for (int pageSize = 10; pageSize <= 50; pageSize += 10)
                {
                    string recentPublicKey = $"recent-public-images_{pageNumber}_{pageSize}";
                    string popularPublicKey = $"popular-public-images_{pageNumber}_{pageSize}";

                    await database.KeyDeleteAsync(recentPublicKey);
                    await database.KeyDeleteAsync(popularPublicKey);
                }
            }
        }
    }
}
