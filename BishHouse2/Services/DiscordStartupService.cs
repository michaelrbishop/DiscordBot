using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BishHouse2.Util;

namespace BishHouse2.Services
{
    public class DiscordStartupService : IHostedService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IConfiguration _config;
        private readonly ILogger<DiscordSocketClient> _logger;

        public DiscordStartupService(DiscordSocketClient discord, IConfiguration config, ILogger<DiscordSocketClient> logger)
        {
            _discord = discord;
            _config = config;
            _logger = logger;

            _discord.Log += msg => LogHelper.OnLogAsync(_logger, msg);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Logging into Discord...");
            await _discord.LoginAsync(TokenType.Bot, _config["Discord:Token"]);

            Console.WriteLine("Starting Discord client...");
            await _discord.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Logging out of Discord...");
            await _discord.LogoutAsync();

            Console.WriteLine("Stopping Discord client...");
            await _discord.StopAsync();
        }
    }
}
