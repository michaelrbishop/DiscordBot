using BishHouse2.Repository.Data;
using BishHouse2.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BishHouse2.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DiscordDBContext _dBContext;
        private readonly IMemoryCache _memoryCache; // TODO : MRB Extract memory cache logic to it's own class
        
        const string AllUsersCacheKey = "AllUsers";

        public UserRepository(DiscordDBContext dBContext, IMemoryCache memoryCache)
        {
            _dBContext = dBContext;
            _memoryCache = memoryCache;
        }
        public async Task Add(User user)
        {
            // TODO : MRB Add user to in-memory cache
            user.CreatedAt = DateTime.UtcNow;
            user.UpdateAt = DateTime.UtcNow;
            await _dBContext.Users.AddAsync(user);
            await SaveAsync();

            AddUserToCache(user);

        }

        public async Task Delete(ulong discordId)
        {
            var user = await GetById(discordId);
            if (user is not null)
            {
                _dBContext.Users.Remove(user);
                await SaveAsync();

                RemoveUserFromCache(discordId);
            }
        }

        public async Task<User?> GetById(ulong discordId)
        {
            User? user = null;
            user = GetUserFromCacheByDiscordId(discordId);
            if (user is not null)
            {
                return user;
            }
            else
            {
                user = await _dBContext.Users.AsNoTracking().SingleOrDefaultAsync(x => x.DiscordId == discordId);
                if (user is not null)
                {
                    AddUserToCache(user);
                }
            }

            return user;
                
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            IEnumerable<User>? users = GetAllUsersFromCache();

            users ??= await _dBContext.Users.AsNoTracking().ToListAsync(); // TODO : MRB Compound assignment operator ??= research

            return users;
        }

        public async Task LoadUserCache()
        {
            var users = await GetAll();
            _memoryCache.Set(AllUsersCacheKey, users);
        }

        public async Task Update(User user)
        {
            user.UpdateAt = DateTime.UtcNow;

            _dBContext.Users.Update(user);
            await SaveAsync();

            UpdateUserInCache(user);

        }

        private async Task SaveAsync()
        {
            await _dBContext.SaveChangesAsync();
        }

        private void AddUserToCache(User user)
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
        }

        private IEnumerable<User>? GetAllUsersFromCache()
        {
            return _memoryCache.Get<IEnumerable<User>>("AllUsers");
        }

        private User? GetUserFromCacheByDiscordId(ulong discordId)
        {
            User? user = null;
            if (_memoryCache.TryGetValue(AllUsersCacheKey, out var userCache))
            {
                if (userCache is not null)
                {
                    user = ((IEnumerable<User>)userCache).SingleOrDefault(x => x.DiscordId == discordId);
                }
            }

            return user;
        }

        private void UpdateUserInCache(User user)
        {
            var userCache = _memoryCache.Get<IEnumerable<User>>(AllUsersCacheKey);
            if (userCache is not null)
            {
                var updatedCache = userCache.Where(x => x.DiscordId != user.DiscordId).ToList();
                updatedCache.Add(user);
                _memoryCache.Set(AllUsersCacheKey, updatedCache);
            }
        }

        private void RemoveUserFromCache(ulong discordId)
        {
            var userCache = _memoryCache.Get<IEnumerable<User>>(AllUsersCacheKey);
            if (userCache is not null)
            {
                var updatedCache = userCache.Where(x => x.DiscordId != discordId).ToList();
                _memoryCache.Set(AllUsersCacheKey, updatedCache);
            }
        }

        public async IAsyncEnumerable<(ulong DiscordId, bool Exists)> CheckIfUsersExists(IEnumerable<ulong> discordIds)
        {
            var users = await GetAll();

            foreach(var discordId in discordIds)
            {
                var exists = users.Any(x => x.DiscordId == discordId);
             
                 yield return (DiscordId: discordId, Exists: exists);
            }
        }

    }
}
