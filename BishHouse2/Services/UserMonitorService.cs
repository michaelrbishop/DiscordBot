using BishHouse2.Components;
using BishHouse2.Repository;
using BishHouse2.Repository.Data;
using BishHouse2.Repository.Factory;
using BishHouse2.Repository.Models;
using BishHouse2.Util;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BishHouse2.Services
{
    public class UserMonitorService : BackgroundService
    {
        private readonly DiscordSocketClient _discord;
        private readonly ILogger _logger;
        private readonly IUserRepository _userRepository;
        private InternalDictionary<(User, DateTime)> _internalDictionary;

        public UserMonitorService(
            DiscordSocketClient discord,
            IRepositoryFactory repositoryFactory, ILogger logger)
        {

            _discord = discord;
            _logger = logger;
            _userRepository = repositoryFactory.CreateRepository<IUserRepository>();
            _internalDictionary = new InternalDictionary<(User, DateTime)>();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _discord.Ready += OnDiscordReady;
            _discord.UserJoined += OnUserJoined;
            _discord.PresenceUpdated += OnPresenceUpdated;
            _discord.MessageReceived += OnDmReceived;

            await RemoveOldEntries(stoppingToken);
        }

        private async Task OnDmReceived(SocketMessage message)
        {
            if (message.Author.IsBot) return;

            if (message.Channel is IDMChannel channel)
            {
                _logger.Information($"{message.Author.Username} sent me a dm.");
                _logger.Information($"{message.Content}");

                await channel.SendMessageAsync("I don't know how to read.");
            }
            else
            {
                if (message.MentionedUsers.Any(x => x.Id == _discord.CurrentUser.Id))
                {
                    _logger.Information($"{message.Author.Username} mentioned me");

                    await message.Channel.SendMessageAsync("I can't read.");
                }

            }
        }

        private async Task OnPresenceUpdated(SocketUser user, SocketPresence before, SocketPresence after)
        {
            // If user is online, check if they're confirmed
            // If they're not confirmed but last updatedat is today, don't ask them
            // If they're not confirmed and they haven't asked today, prompt them

            // TODO : MRB
            // We could possibly look at the difference in active clients between
            // the before and after. We could use that to calculate a collective status change

            // We don't care about bots
            if (user.IsBot) return;

            if (_internalDictionary.TryGetValue(user.Id, out _))
            {
                // User is already being processed
                // We don't need to send the same message to a user
                // if their status changes on multiple clients (desktop, mobile, etc)
                return;
            }
            
            if (after.Status == UserStatus.Online)
            {
                var dbUser = await _userRepository.GetById(user.Id);

                if (dbUser is not null && !dbUser.IsConfirmed)
                {
                    
                    if (DateOnly.FromDateTime(dbUser.UpdateAt) < DateOnly.FromDateTime(DateTime.UtcNow)) // TODO : MRB Use confirm at
                    {
                        _logger.Information($"I sent a dm for more info to {user.Username}");

                        try
                        {
                            await user.SendMessageAsync(
                                    components: UserInfoForm.InitialComponent((SocketGuildUser)user).Build(),
                                    isTTS: true);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Error sending DM to {user.Username}");
                        }
                        

                        // The repository will update the UpdateAt field
                        await _userRepository.Update(dbUser);
                    }
                }
            }
        }

        private async Task OnUserJoined(SocketGuildUser user)
        {
            // Check if user exists in db, if not add them
            // Then prompt them to fill out their info

            // We don't care about bots
            if (user.IsBot) return;

            var dbUser = await _userRepository.GetById(user.Id);
            if (dbUser is null)
            {
                await AddUser(user);

                await user.SendMessageAsync(
                    components: UserInfoForm.InitialComponent(user).Build(),
                    isTTS: true);
            }
        }

        private Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState beforeState, SocketVoiceState afterState)
        {
            if (beforeState.VoiceChannel == null && afterState.VoiceChannel != null)
            {
                Console.WriteLine($"{user.Username} joined voice channel {afterState.VoiceChannel.Name}");
            }

            if (beforeState.VoiceChannel != null && afterState.VoiceChannel == null)
            {
                Console.WriteLine($"{user.Username} left voice channel {beforeState.VoiceChannel.Name}");
                
            }

            return Task.CompletedTask;
        }

        private async Task OnMessageRecieved(SocketMessage message) // TODO : MRB We should get rid of this
        {
            var dbUser = await _userRepository.GetById(message.Author.Id);

            if (dbUser is not null && dbUser.IsConfirmed) return;

            if (message.Author is SocketGuildUser guildUser)
            {
                if (guildUser.IsBot) return;

                 await CheckIfUserExists(guildUser);

                // Only proceed if the message is from a user (not a bot) and is in a guild channel
                if (message.Channel is SocketTextChannel textChannel)
                {
                    //var user = (IUser)guildUser; // TODO : MRB Use to send DM to user instead of the channel

                    //await user.SendMessageAsync(components: UserInfoForm.InitialComponent(guildUser).Build(),
                    //    isTTS: true);
                    await textChannel.SendMessageAsync(
                        components: UserInfoForm.InitialComponent(guildUser).Build(),
                        isTTS: true
                    );
                }
            }
        }

        private async Task OnDiscordReady()
        {
            // When we fire up the first time, look at the users of the guild
            // If any are missing from the db, add them to the db
            var guild = _discord.Guilds.First();

            await _userRepository.LoadUserCache();

            // TODO : MRB Get db users
            // Compare guild users to db users
            // If any are missing, add them to db
            // Update in-memory application user cache
            await CheckIfUsersExist(guild.Users.Where(x => !x.IsBot));

            // TODO : MRB Fire off function that will run on a timer and look for unknown users

        }

        private async Task CheckIfUsersExist(IEnumerable<SocketGuildUser> guildUsers)
        {
            await foreach(var guildUserResult in _userRepository.CheckIfUsersExists(guildUsers.Select(x => x.Id)))
            {
                
                if (!guildUserResult.Exists)
                {

                    var guildUser = guildUsers.First(x => x.Id == guildUserResult.DiscordId);
                    
                    await AddUser(guildUser);
                }
            }
        }

        private async Task CheckIfUserExists(SocketGuildUser guildUser)
        {
            if (guildUser.IsBot) return;

            var user = await _userRepository.GetById(guildUser.Id);
            if (user is null)
            {
                await AddUser(guildUser);
            }
        }

        private async Task AddUser(SocketGuildUser guildUser)
        {

            User newUser = new()
            {
                DiscordId = guildUser.Id,
                GuildId = guildUser.Guild.Id,
                FirstName = guildUser.Username,
                LastInitial = guildUser.Username.First(),
                System = string.Empty,
                IsConfirmed = false,
            };

            await _userRepository.Add(newUser);
        }

        private async Task RemoveOldEntries(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    _internalDictionary.RemoveBy(entry => (DateTime.UtcNow - entry.Item2).TotalMinutes > 10);
                    await Task.Delay(TimeSpan.FromMinutes(5), token);
                }
            }
            catch (TaskCanceledException)
            {
                // Gracefully handle cancellation, no action needed
                _logger.Information("UserMonitorService cancellation requested.");
            }
        }
    }
}
