using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;

namespace BishHouse2.Services
{
    public class VoiceChannelMonitorService : BackgroundService
    {
        private readonly DiscordSocketClient _discord;

        public VoiceChannelMonitorService(DiscordSocketClient discord)
        {
            _discord = discord;           
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _discord.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _discord.UserVoiceStateUpdated -= OnUserVoiceStateUpdated;
            await base.StopAsync(cancellationToken);
        }

        private async Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState beforeState, SocketVoiceState afterState)
        {
            Console.WriteLine($"{user.Username} changed voice state from {beforeState.VoiceChannel?.Name ?? "none"} to {afterState.VoiceChannel?.Name ?? "none"}");
            Console.WriteLine("");

            // if beforeState.VoiceChannel is null and afterState.VoiceChannel is not null, user has joined a voice channel
            // if beforeState.VoiceChannel is not null and afterState.VoiceChannel is null, user has left a voice channel
            
            var voiceChannelId = afterState.VoiceChannel?.Id ?? beforeState.VoiceChannel?.Id ?? 0;
            var hasEntered = beforeState.VoiceChannel?.Id is null && afterState.VoiceChannel?.Id is not null;

            // If user is a certain user from the db and voiceChannelId is not 0
            // If the user has entered the chat, mute them and play a fart sound
            // If the user has left the chat, play a flushing sound

            if(voiceChannelId != 0)
            {
                IMessageChannel channel = (IMessageChannel)await _discord.GetChannelAsync(voiceChannelId);

                if (hasEntered)
                {
                    await channel.SendMessageAsync($"{user.Mention} has entered {afterState.VoiceChannel?.Name}");
                }
                else
                {
                    await channel.SendMessageAsync($"::{user.Mention} has left the chat::");
                }
            }

        }
    }
}
