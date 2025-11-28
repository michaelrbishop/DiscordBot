using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BishHouse2.Repository.Models
{
    public class User
    {
        public int Id { get; set; }
        public required ulong DiscordId { get; set; }
        public required ulong GuildId { get; set; }
        public required string FirstName { get; set; }
        public required char LastInitial { get; set; }
        public required string System { get; set; }
        public required bool IsConfirmed { get; set; }
        public DateTime ConfirmedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }
    }
}
