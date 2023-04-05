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
        /// Handle Status Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="UserIsAdmin"></param>
        /// <returns></returns>
        static Task HandleCommand_Status(SocketSlashCommand command, bool UserIsAdmin)
        {
            MyCommandHandling.RespondAsync(command, Helper.DiscordHelper.BuildEmbed(command, "Version: " + Globals.MyVersion + "\n\nThe bot has been alive and healthy for:\n" + GetFormattedStringFromTS(Globals.sw.Elapsed), null, Helper.DiscordHelper.EmbedColors.NormalEmbed));

            return Task.CompletedTask;
        }



        /// <summary>
        /// Gets a formatted String from a Timespan
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static string GetFormattedStringFromTS(TimeSpan ts)
        {
            const string separator = ", ";
            string rtrn = "";

            try
            {
                if (ts.TotalMilliseconds < 1) { return "No time"; }

                rtrn = string.Join(separator, new string[]
                {
                ts.Days > 0 ? ts.Days + (ts.Days > 1 ? " days" : " day") : null,
                ts.Hours > 0 ? ts.Hours + (ts.Hours > 1 ? " hours" : " hour") : null,
                ts.Minutes > 0 ? ts.Minutes + (ts.Minutes > 1 ? " minutes" : " minute") : null,
                ts.Seconds > 0 ? ts.Seconds + (ts.Seconds > 1 ? " seconds" : " second") : null,
                ts.Milliseconds > 0 ? ts.Milliseconds + (ts.Milliseconds > 1 ? " milliseconds" : " millisecond") : null,
                }.Where(t => t != null));
            }
            catch (Exception ex)
            {
                rtrn = "Failed creating Uptime string.";
            }

            return rtrn;
        }
    }
}
