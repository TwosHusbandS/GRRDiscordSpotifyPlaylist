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
            string output = "";
            output += "Every Song posted in #was-ich-höre is added to our Spotify Playlist.";
            output += "\n\n";
            output += "Our playlist holds ";
            output += Options.SPOTIFY_PLAYLIST_NUMBER_OF_SONGS.ToString();
            output += " songs (oldest one gets removed when a new one is added) and each User can add up to ";
            output += Options.USER_DAILY_LIMIT;
            output += " Songs per day.";
            output += "\n\n";
            output += "The bot can handle Spotify Links, Youtube Links as well as .s and .fm commands from the FMBot.";
            output += "\n\n";
            output += "Playlist Link:";
            output += "\n";
            output += Globals.SpotifyPlaylist;
            output += "\n";
            output += "(You can also click the blue text at the top of this message to get to the playlist)";
            MyCommandHandling.RespondAsync(command, Helper.DiscordHelper.BuildEmbed(command, output, null, Helper.DiscordHelper.EmbedColors.NormalEmbed));
            return Task.CompletedTask;
        }
    }
}
