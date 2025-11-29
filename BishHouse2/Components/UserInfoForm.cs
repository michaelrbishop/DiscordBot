using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BishHouse2.Components
{
    public static class UserInfoForm
    {
        public const string MODALCREATE = "Create";
        public const string MODALUPDATE = "Update";

        /// <summary>
        /// Build the user info modal
        /// </summary>
        /// <param name="user"></param>
        /// <param name="modalMode">This can only be Create or Update. Best if accessed through const string in UserInfoForm MODALCREATE or MODALUPDATE</param>
        /// <returns></returns>
        public static ModalBuilder BuildModal(string modalMode, string? firstName = null, string? lastInitial = null, string? system = null)
        {

            var firstNameInput = new TextInputBuilder()
                                     .WithCustomId(Component.UserInfoFirstName.ToString())
                                     .WithPlaceholder("First name")
                                     .WithLabel("First Name")
                                     .WithMinLength(1)
                                     .WithRequired(true);
            var lastNameInput = new TextInputBuilder()
                                      .WithCustomId(Component.UserInfoLastInitial.ToString())
                                      .WithPlaceholder("Last initial")
                                      .WithLabel("Last Initial")
                                      .WithMinLength(1)
                                      .WithMaxLength(1)
                                      .WithRequired(true);
            var systemInput = new TextInputBuilder()
                                      .WithCustomId(Component.UserInfoSystem.ToString())
                                      .WithPlaceholder("Xbox, Playstation, PC")
                                      .WithLabel("Gaming System")
                                      .WithMinLength(1)
                                      .WithRequired(true);

            if (!string.IsNullOrEmpty(firstName))
            {
                firstNameInput.WithValue(firstName);
            }

            if (!string.IsNullOrEmpty(lastInitial))
            {
                lastNameInput.WithValue(lastInitial);
            }

            if (!string.IsNullOrWhiteSpace(system))
            {
                systemInput.WithValue(system);
            }


            var builder = new ModalBuilder()
                .WithTitle("User Information Form")
                .WithCustomId($"{Component.UserInfoModal}-{modalMode}")
                .AddTextInput(firstNameInput)
                .AddTextInput(lastNameInput)
                .AddTextInput(systemInput);


            return builder;
        }
        public static ComponentBuilderV2 InitialComponent(SocketGuildUser user)
        {
            var builder = new ComponentBuilderV2()
                    .WithTextDisplay(
                    $"I'm from Modern Asbestos & More. {user.Mention}, I don't know who the fuck you are. Fill out the form so I know.");

            var buttonBuilder = new ButtonBuilder("Sell your soul!")
                                        .WithStyle(ButtonStyle.Primary)
                                        .WithCustomId($"{Component.UserInfoInitialButton}-{user.Id}");

            builder.WithActionRow([buttonBuilder]);               

            
            return builder;

        }
    }
}
