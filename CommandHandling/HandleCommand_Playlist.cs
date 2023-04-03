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
        /// <summary>
        /// Handle Playlist Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="UserIsAdmin"></param>
        /// <returns></returns>
        static Task HandleCommand_Playlist(SocketSlashCommand command, bool UserIsAdmin)
        {
            string tmp = "You can also click the blue text at the top of this message.\nAlso make sure to follow the playlist and the GRR Spotify account!\n\n" + Globals.SpotifyPlaylist;
            MyCommandHandling.RespondAsync(command, Helper.DiscordHelper.BuildEmbed(command, tmp, null, Helper.DiscordHelper.EmbedColors.NormalEmbed));
            return Task.CompletedTask;
        }
    }
}
