using BishHouse2.Components;
using BishHouse2.Repository;
using BishHouse2.Repository.Models;
using BishHouse2.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BishHouse2.Modules
{
    public class UserCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IUserRepository _userRepository;
        private readonly IDadJokeService _dadJokeService;

        public UserCommandModule(IUserRepository userRepository, IDadJokeService dadJokeService)
        {
            _userRepository = userRepository;
            _dadJokeService = dadJokeService;
        }

        [SlashCommand("change-my-nickname", "Change your display name for this server")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        [RequireBotPermission(GuildPermission.ChangeNickname)]
        public async Task ChangeUserNickname(string nickname)
        {
            var user = (SocketGuildUser)Context.User;

            await user.ModifyAsync(x => x.Nickname = nickname);
            await RespondAsync($"{user.Mention} your display name has changed to {nickname}");
        }

        [SlashCommand("who-is", "Get info on a user. The message sent is only viewable by you.")]
        [RequireContext(ContextType.Guild)]
        public async Task WhoIsThis(SocketGuildUser user)
        {
            var msg = string.Empty;
            var dbUser = await _userRepository.GetById(user.Id);
            if (dbUser is null)
            {
                msg = $"I don't fucking know who {user.Mention} is. I just work here.";
            }
            else
            {
                msg = $"{user.Mention} is known as {dbUser.FirstName} {dbUser.LastInitial}. They are running the {dbUser.System} system.";
            }


            await RespondAsync(msg, ephemeral: true);
        }

        [SlashCommand("update-my-info", "Update your user information")]
        [RequireContext(ContextType.Guild)]
        public async Task UpdateMyInfo()
        {
            var user = (SocketGuildUser)Context.User;
            var userModel = await _userRepository.GetById(user.Id);
            var modal = UserInfoForm.BuildModal(
                UserInfoForm.MODALUPDATE,
                userModel?.FirstName,
                userModel?.LastInitial.ToString() ?? null,
                userModel?.System ?? null
            );
            await Context.Interaction.RespondWithModalAsync(modal.Build());
        }

        [SlashCommand("dad-joke", "Get a random dad joke")]
        [RequireContext(ContextType.Guild)]
        public async Task GetDadJoke()
        {
            var joke = await _dadJokeService.GetRandomDadJoke();
            await RespondAsync(joke, isTTS: true);
        }
    }
}
