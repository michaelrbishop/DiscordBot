using BishHouse2.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BishHouse2.Services
{
    public interface IUserCacheService // TODO: MRB We likely don't need this
    {
        Task AddUserToCache(User user);
        Task<User?> GetUserFromCacheByDiscordId(ulong discordId);
        Task UpdateUserInCache(User user);
        Task RemoveUserInCache(ulong discordId);
        Task<IEnumerable<User>?> GetAllUsers();
        Task LoadCache(IEnumerable<User>? users);

    }
}
