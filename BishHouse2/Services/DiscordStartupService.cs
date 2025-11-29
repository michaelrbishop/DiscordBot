using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using BishHouse2.Util;
using Serilog;

namespace BishHouse2.Services
{
    public class DiscordStartupService : IHostedService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public DiscordStartupService(DiscordSocketClient discord, IConfiguration config, ILogger logger)
        {
            _discord = discord;
            _config = config;
            _logger = logger;

            _discord.Log += msg => LogHelper.OnLogAsync(_logger, msg);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Logging into Discord...");
            await _discord.LoginAsync(TokenType.Bot, _config["Discord:Token"]);

            _logger.Information("Starting Discord client...");
            await _discord.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Logging out of Discord...");
            await _discord.LogoutAsync();

            _logger.Information("Stopping Discord client...");
            await _discord.StopAsync();
        }
    }
}
