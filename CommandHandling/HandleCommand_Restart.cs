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
        /// Handle Restart Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="UserIsAdmin"></param>
        /// <returns></returns>
        static async Task HandleCommand_Restart(SocketSlashCommand command, bool UserIsAdmin)
        {
            /// Permission check
            if (UserIsAdmin)
            {
                await MyCommandHandling.RespondAsync(command, Helper.DiscordHelper.BuildEmbed(command, "Will try restarting the Bot...Wish me luck lmao", null, Helper.DiscordHelper.EmbedColors.NormalEmbed));
                await Task.Delay(500);
                Helper.LinuxHelper.RestartService();
            }
            else
            {
                MissingPermissions(command);
            }
        }
    }
}
