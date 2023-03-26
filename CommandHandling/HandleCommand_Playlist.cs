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
        static Task HandleCommand_Playlist(SocketSlashCommand command, bool UserIsAdmin)
        {
            string tmp = "You can also click the blue text at the top of this message.\nAlso make sure to follow the playlist and the GRR Spotify account!\n\n" + Logic.SpotifyPlaylist;
            command.RespondAsync(embed: Globals.BuildEmbed(command, tmp, null, Globals.EmbedColors.NormalEmbed));
            return Task.CompletedTask;
        }
    }
}
