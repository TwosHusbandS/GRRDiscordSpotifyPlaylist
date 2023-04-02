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
        /// <summary>
        /// Handle Shutdown Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="UserIsAdmin"></param>
        /// <returns></returns>
        static async Task HandleCommand_Shutdown(SocketSlashCommand command, bool UserIsAdmin)
        {
            // Permission check
            if (UserIsAdmin)
            {
                await command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Will try to Shutdown the Bot...Wish me luck lmao", null, Helper.DiscordHelper.EmbedColors.NormalEmbed));
                await Task.Delay(500);
                Helper.LinuxHelper.ShutdownService();
            }
            else
            {
                MissingPermissions(command);
            }
        }
    }
}
