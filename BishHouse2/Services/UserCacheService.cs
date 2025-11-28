using BishHouse2.Repository.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BishHouse2.Services
{
    public class UserCacheService : IUserCacheService // TODO : MRB we likely don't need this
    {
        private readonly IMemoryCache _memoryCache;

        const string AllUsersCacheKey = "AllUsers";

        public UserCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task AddUserToCache(User user)
        {
            var userCache = _memoryCache.Get<IEnumerable<User>>(AllUsersCacheKey);
            List<User> updatedCache;
            if (userCache is not null)
            {
                updatedCache = userCache.ToList();
                updatedCache.Add(user);
            }
            else
            {
                updatedCache = new List<User> { user };
            }

            _memoryCache.Set(AllUsersCacheKey, updatedCache);

            return Task.CompletedTask;
        }

        public Task RemoveUserInCache(ulong discordId)
        {
            var userCache = _memoryCache.Get<IEnumerable<User>>(AllUsersCacheKey);
            if (userCache is not null)
            {
                var updatedCache = userCache.Where(x => x.DiscordId != discordId).ToList();
                _memoryCache.Set(AllUsersCacheKey, updatedCache);
            }
            return Task.CompletedTask;
        }

        public Task<IEnumerable<User>?> GetAllUsers()
        {
            return Task.FromResult(_memoryCache.Get<IEnumerable<User>>(AllUsersCacheKey));
        }

        public Task<User?> GetUserFromCacheByDiscordId(ulong discordId)
        {
            User? user = null;
            if (_memoryCache.TryGetValue(AllUsersCacheKey, out var userCache))
            {
                if (userCache is not null)
                {
                    user = ((IEnumerable<User>)userCache).SingleOrDefault(x => x.DiscordId == discordId);
                }
            }

            return Task.FromResult(user);
        }

        public Task UpdateUserInCache(User user)
        {
            var userCache = _memoryCache.Get<IEnumerable<User>>(AllUsersCacheKey);
            if (userCache is not null)
            {
                var updatedCache = userCache.Where(x => x.DiscordId != user.DiscordId).ToList();
                updatedCache.Add(user);
                _memoryCache.Set(AllUsersCacheKey, updatedCache);
            }

            return Task.CompletedTask;
        }

        public Task LoadCache(IEnumerable<User>? users)
        {
            _memoryCache.Set(AllUsersCacheKey, users);
            return Task.CompletedTask;
        }
    }
}
