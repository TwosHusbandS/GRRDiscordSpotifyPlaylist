﻿using Discord.WebSocket;
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
        /// Handle Help Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="UserIsAdmin"></param>
        /// <returns></returns>
        static Task HandleCommand_Help(SocketSlashCommand command, bool UserIsAdmin)
        {
            List<KeyValuePair<string, string>> MyList = new List<KeyValuePair<string, string>>();
            MyList.Add(new KeyValuePair<string, string>("Showing all Commands", GetHelpCommandOutput()));
            MyCommandHandling.RespondAsync(command, Helper.DiscordHelper.BuildEmbed(command, null, MyList, Helper.DiscordHelper.EmbedColors.NormalEmbed));

            return Task.CompletedTask;
        }


        /// <summary>
        /// Generate Output for the Help Command
        /// </summary>
        /// <returns></returns>
        static string GetHelpCommandOutput()
        {
            string HelpOutput = "";
            HelpOutput += "/help";
            HelpOutput += "\n/status";
            HelpOutput += "\n/playlist";
            HelpOutput += "\n";
            HelpOutput += "\n/songs list";
            HelpOutput += "\n/songs add {track}";
            HelpOutput += "\n/songs remove-at-index {index}";
            HelpOutput += "\n/songs remove-song {track}";
            HelpOutput += "\n";
            HelpOutput += "\n/backups list";
            HelpOutput += "\n/backups create {name}";
            HelpOutput += "\n/backups apply {name}";
            HelpOutput += "\n/backups delete {name}";
            HelpOutput += "\n";
            HelpOutput += "\n/settings show";
            HelpOutput += "\n/settings limit {limit_type} {number}";
            HelpOutput += "\n/settings channel {channel_type} {name}";
            HelpOutput += "\n/settings color {color_type} {name}";
            HelpOutput += "\n/settings log {log_type} {log_level}";
            HelpOutput += "\n/settings maximum-levenshtein-distance {levenshtein-distance}";
            HelpOutput += "\n/settings spotify_playlist {keep_songs} {playlist}";
            HelpOutput += "\n/settings discord_guild_id {guild_id}";
            HelpOutput += "\n/settings show-activity-internal {true/false}";
            HelpOutput += "\n";
            HelpOutput += "\n/dailyuserlimit show all";
            HelpOutput += "\n/dailyuserlimit show user {user}";
            HelpOutput += "\n/dailyuserlimit reset all";
            HelpOutput += "\n/dailyuserlimit reset user {user}";
            HelpOutput += "\n";
            HelpOutput += "\n/shutdown";
            HelpOutput += "\n/reset";

            return HelpOutput;
        }
    }
}
