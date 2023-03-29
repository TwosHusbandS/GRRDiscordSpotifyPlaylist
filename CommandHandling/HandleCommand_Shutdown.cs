using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WasIchHoerePlaylist.CommandHandling
{
    internal partial class MyCommandHandling
    {
        static async Task HandleCommand_Shutdown(SocketSlashCommand command, bool UserIsAdmin)
        {
            if (UserIsAdmin)
            {
                await command.RespondAsync(embed: Globals.BuildEmbed(command, "Will try to Shutdown the Bot...Wish me luck lmao", null, Globals.EmbedColors.NormalEmbed));
                await Task.Delay(500);
                Globals.ExecuteLinuxCommand("systemctl stop grrdiscordspotifybot");
            }
            else
            {
                MissingPermissions(command);
            }
        }
    }
}
