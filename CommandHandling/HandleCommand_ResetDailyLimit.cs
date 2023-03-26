using Discord.WebSocket;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WasIchHoerePlaylist.CommandHandling
{
    internal partial class MyCommandHandling
    {
        static Task HandleCommand_ResetDailyLimit(SocketSlashCommand command, bool UserIsAdmin)
        {
            try
            {
                if (!UserIsAdmin)
                {
                    MissingPermissions(command);
                    return Task.CompletedTask;
                }

                string output = "";
                switch (command.Data.Options.First().Name)
                {
                    case "all":
                        UserSongs.Reset(0);
                        output = "Reset the daily limit of all Users.";
                        break;
                    case "user":
                        SocketGuildUser SGU = (SocketGuildUser)command.Data.Options.First().Options.First().Value;
                        UserSongs.Reset(SGU.Id);
                        output = "Reset the daily limit of '" + SGU.Nickname + " (" + SGU.Username + ")'";
                        break;
                }
                command.RespondAsync(embed: Globals.BuildEmbed(command, output, null, Globals.EmbedColors.NormalEmbed));
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Error while resetting Limits", null, Globals.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
            return Task.CompletedTask;
        }
    }
}