using BishHouse2.Components;
using BishHouse2.Repository;
using BishHouse2.Repository.Factory;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BishHouse2.Services
{
    public class ComponentInteractionHandlingService : BackgroundService
    {
        private readonly DiscordSocketClient _discord;
        private readonly ILogger _logger;
        private readonly IUserRepository _userRepository;

        public ComponentInteractionHandlingService(DiscordSocketClient discord,
            IRepositoryFactory repositoryFactory, ILogger logger)
        {
            _discord = discord;
            _logger = logger;
            _userRepository = repositoryFactory.CreateRepository<IUserRepository>();
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _discord.ModalSubmitted += OnModalSubmitted;
            _discord.InteractionCreated += OnComponentInteractionAsync;

            return Task.CompletedTask;
        }

        private async Task OnModalSubmitted(SocketModal modal)
        {
            // TODO : MRB We could use mediatr here to decouple the handling of different modals
            if (modal.Data.CustomId.Contains(Component.UserInfoModal.ToString()))
            {
                var guildUser = modal.User as SocketGuildUser;
               
                var firstName = modal.Data.Components
                    .FirstOrDefault(x => x.CustomId == Component.UserInfoFirstName.ToString())
                    ?.Value;

                var lastInitial = modal.Data.Components
                    .FirstOrDefault(x => x.CustomId == Component.UserInfoLastInitial.ToString())
                    ?.Value;
                var system = modal.Data.Components
                    .FirstOrDefault(x => x.CustomId == Component.UserInfoSystem.ToString())
                    ?.Value;

                var dbUser = await _userRepository.GetById(modal.User.Id);

                dbUser!.FirstName = firstName!;
                dbUser!.LastInitial = lastInitial![0];
                dbUser!.System = system!;
                dbUser!.IsConfirmed = true;
                dbUser!.ConfirmedAt = DateTime.UtcNow;

                await _userRepository.Update(dbUser);

                await modal.RespondAsync($"Thanks {modal.User.Mention}. Your information has been recorded. Also, fuck you.", isTTS: true, ephemeral: true);

            }
        }

        private async Task OnComponentInteractionAsync(SocketInteraction interaction)
        {
            var user = interaction.User;

            // Handle component interactions here
            if (interaction is SocketMessageComponent component)
            {
                // TODO : MRB Create a class to process SocketMessageComponent types.
                if (component.Data.CustomId.Contains(Component.UserInfoInitialButton.ToString())) 
                {
                    var customId = component.Data.CustomId;
                    var userIdString = customId.Substring(customId.IndexOf("-") + 1);
                    var dbUser = await _userRepository.GetById(user.Id);

                    if (string.Equals(userIdString, user?.Id.ToString()) && dbUser is not null)
                    {
                        await component.RespondWithModalAsync(UserInfoForm.BuildModal(UserInfoForm.MODALUPDATE, dbUser.FirstName, 
                                                                    dbUser.LastInitial.ToString(), dbUser.System).Build());
                    }
                    else
                    {
                        await interaction.RespondAsync("That wasn't for you, dipshit.", isTTS: true, ephemeral: true);
                    }
                }

            }
        }
    }
}
