using BishHouse2.Util;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Reflection;

namespace BishHouse2.Services
{
    public class InteractionHandlingService : IHostedService
    {
        private readonly DiscordSocketClient _discord;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public InteractionHandlingService(
            DiscordSocketClient discord,
            InteractionService interactions,
            IServiceProvider services,
            IConfiguration config,
            ILogger logger)
        {
            _discord = discord;
            _interactions = interactions;
            _services = services;
            _config = config;
            _logger = logger;

            _interactions.Log += msg => LogHelper.OnLogAsync(_logger, msg);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            _discord.Ready += () => _interactions.RegisterCommandsGloballyAsync(true);
            _discord.InteractionCreated += OnInteractionAsync;

            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }



        public Task StopAsync(CancellationToken cancellationToken)
        {
            _interactions.Dispose();
            return Task.CompletedTask;
        }

        private async Task OnInteractionAsync(SocketInteraction interaction)
        {
            try
            {
                if (interaction is not SocketMessageComponent && interaction.Type != InteractionType.ModalSubmit) //  && interaction.Type is not InteractionType.ModalSubmit // TODO : MRB
                {

                    Console.WriteLine("Interaction received: " + interaction.Type);
                    Console.WriteLine("");
                    var context = new SocketInteractionContext(_discord, interaction);
                    var result = await _interactions.ExecuteCommandAsync(context, _services);

                    if (!result.IsSuccess)
                    {
                        await context.Channel.SendMessageAsync(result.ToString());
                    }

                }

            }
            catch
            {
                if (interaction.Type == InteractionType.ApplicationCommand)
                {
                    await interaction.GetOriginalResponseAsync()
                        .ContinueWith(msg => msg.Result.DeleteAsync());
                }
            }
        }

    }
}
