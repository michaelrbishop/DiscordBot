using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BishHouse2
{
    public static class ServiceExtensions
    {
        public static void ConfigureDiscordSocketClient(this IServiceCollection services)
        {
            var discord = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged
                                | GatewayIntents.GuildMessages
                                | GatewayIntents.GuildMessageReactions
                                | GatewayIntents.GuildMembers
                                | GatewayIntents.GuildPresences
                                | GatewayIntents.MessageContent
                                | GatewayIntents.GuildVoiceStates

            });

            //discord.UserJoined += async (user) => // TODO : MRB Get rid of this
            //{
            //    // Example user joined handling
            //    Console.WriteLine($"{user.Username} has joined the server.");
            //    await Task.CompletedTask;
            //};

            //discord.UserVoiceStateUpdated += async (user, before, after) =>
            //{
            //    // Example voice state update handling
            //    Console.WriteLine($"{user.Username} changed voice state from {before.VoiceChannel?.Name ?? "none"} to {after.VoiceChannel?.Name ?? "none"}");
            //    await Task.CompletedTask;
            //};

            //discord.PresenceUpdated += async (user, before, after) =>
            //{
            //    // Example presence update handling
            //    Console.WriteLine($"{user.Username} changed presence from {before?.Status} to {after.Status}");
            //    await Task.CompletedTask;
            //};
            services.AddSingleton<DiscordSocketClient>(discord);

        }
    }
}
