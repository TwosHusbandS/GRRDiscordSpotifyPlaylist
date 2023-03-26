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
        static Task HandleCommand_Status(SocketSlashCommand command, bool UserIsAdmin)
        {
            command.RespondAsync(embed: Globals.BuildEmbed(command, "The bot has been alive and healthy for:\n" + Globals.ToFormattedString(Globals.sw.Elapsed), null, Globals.EmbedColors.NormalEmbed));
            return Task.CompletedTask;
        }

    }
}
