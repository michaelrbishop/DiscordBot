using BishHouse2.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BishHouse2.Repository
{
    public interface IUserRepository : IRepository<User, ulong>
    {
        IAsyncEnumerable<(ulong DiscordId, bool Exists)> CheckIfUsersExists(IEnumerable<ulong> discordIds);
        //Task<User?> GetUserByDiscordId(ulong discordId);
        //Task<IEnumerable<User>> GetUsers();
        //Task AddUser(User user);
        //Task UpdateUser(User user);
        //Task DeleteUser(ulong discordId);
        Task LoadUserCache();
    }
}
