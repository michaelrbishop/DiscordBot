using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BishHouse2.Services
{
    public interface IDadJokeService
    {
        Task<string> GetRandomDadJoke();
    }
}
