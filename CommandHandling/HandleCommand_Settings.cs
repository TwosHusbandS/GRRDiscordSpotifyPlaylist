using Discord.WebSocket;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WasIchHoerePlaylist.CommandHandling
{
    internal partial class MyCommandHandling
    {
        /// <summary>
        /// Handle Settings Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="UserIsAdmin"></param>
        /// <returns></returns>
        static Task HandleCommand_Settings(SocketSlashCommand command, bool UserIsAdmin)
        {
            if (!UserIsAdmin)
            {
                MissingPermissions(command);
                return Task.CompletedTask;
            }

            List<KeyValuePair<string, object>> parameter = new List<KeyValuePair<string, object>>();
            try
            {
                List<SocketSlashCommandDataOption> Parameters = command.Data.Options.First().Options.ToList<SocketSlashCommandDataOption>();
                foreach (var Parameter in Parameters)
                {
                    parameter.Add(new KeyValuePair<string, object>(Parameter.Name, Parameter.Value));
                }

                switch (command.Data.Options.First().Name)
                {
                    case "show":
                        HandleSettings_Show(command);
                        break;
                    case "re-read":
                        HandleSettings_ReRead(command);
                        break;
                    case "limit":
                        HandleSettings_Limit(command, parameter);
                        break;
                    case "channel":
                        HandleSettingsCommand_Channel(command, parameter);
                        break;
                    case "color":
                        HandleSettingsCommand_Color(command, parameter);
                        break;
                    case "log":
                        HandleSettingsCommand_LogLevel(command, parameter);
                        break;
                    case "spotify-playlist":
                        HandleSettingsCommand_Playlist(command, parameter);
                        break;
                    case "maximum-levenshtein-distance":
                        HandleSettingsCommand_Levenshtein(command, parameter);
                        break;
                    case "discord-guild-id":
                        HandleSettingsCommand_DiscordGuildId(command, parameter);
                        break;
                    case "show-activity-internal":
                        HandleSettingsCommand_ShowActivityInternal(command, parameter);
                        break;
                }
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
                command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error processing Settings command", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
            }
            return Task.CompletedTask;

        }

        /// <summary>
        /// Handle Settings Show Command
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        static async Task HandleSettings_Show(SocketSlashCommand command)
        {
            // print all non-sensitive settings to channel
            List<KeyValuePair<string, string>> myList = new List<KeyValuePair<string, string>>();
            myList.Add(new KeyValuePair<string, string>("Showing all Settings", Options.SettingsListCommandOutput()));
            command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, null, myList, Helper.DiscordHelper.EmbedColors.NormalEmbed));
        }

        /// <summary>
        /// Handle Settings ReRead Command
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        static async Task HandleSettings_ReRead(SocketSlashCommand command)
        {
            // re-read Settings from File again
            Options.ReadFromFile();
            List<KeyValuePair<string, string>> myList = new List<KeyValuePair<string, string>>();
            string Description = "Sucessfully Re-Read all Settings from file.\nNew Settings below:\n\n";
            myList.Add(new KeyValuePair<string, string>("Showing all (new) Settings", Options.SettingsListCommandOutput()));
            command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, Description, myList, Helper.DiscordHelper.EmbedColors.NormalEmbed));
        }

        static Task HandleSettings_Limit(SocketSlashCommand command, List<KeyValuePair<string, object>> Parameter)
        {
            try
            {
                int Number = 0;
                if (Int32.TryParse(Parameter[1].Value.ToString(), out Number))
                {
                    // parsing number, making sure its valid
                    if (0 < Number && Number < 501)
                    {
                        string OptionName = "";
                        if (Parameter[0].Value.ToString() == "1")
                        {
                            Options.SPOTIFY_PLAYLIST_NUMBER_OF_SONGS = Number;
                            APIs.MySpotify.KeepSongLimit();
                            OptionName = "playlist_amount_of_songs";
                        }
                        else if (Parameter[0].Value.ToString() == "2")
                        {
                            Options.USER_DAILY_LIMIT = Number;
                            OptionName = "user_daily_limit";
                        }

                        // actully writing to file and outputting
                        Options.WriteToFile();
                        command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Changed '" + OptionName + "' to: '" + Number + "'", null, Helper.DiscordHelper.EmbedColors.NormalEmbed));
                    }
                    else
                    {
                        command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error changing Limit Setting (Number not above zero)", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                    }
                }
                else
                {
                    command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error changing Limit Setting (TryParse)", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                }
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error changing Limit Setting", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }

            return Task.CompletedTask;
        }



        /// <summary>
        /// Handle Settings Channel Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="Parameter"></param>
        /// <returns></returns>
        static Task HandleSettingsCommand_Channel(SocketSlashCommand command, List<KeyValuePair<string, object>> Parameter)
        {
            try
            {
                SocketGuildChannel channel = (SocketGuildChannel)Parameter[1].Value;
                string OptionName = "";

                if (Parameter[0].Value.ToString() == "1")
                {
                    Options.DISCORD_PUBLIC_CHANNEL = channel.Id;
                    OptionName = "public-channel";
                }
                else if (Parameter[0].Value.ToString() == "2")
                {
                    Options.DISCORD_INTERNAL_CHANNEL = channel.Id;
                    OptionName = "internal-channel";
                }

                // actully writing to file and outputting
                Options.WriteToFile();
                command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Changed '" + OptionName + "' to: '#" + channel.Name + "'", null, Helper.DiscordHelper.EmbedColors.NormalEmbed));
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error changing Channel Setting", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// Handle Settings Color Command 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="Parameter"></param>
        /// <returns></returns>
        static Task HandleSettingsCommand_Color(SocketSlashCommand command, List<KeyValuePair<string, object>> Parameter)
        {
            try
            {
                // Trying to parse color
                System.Drawing.Color newColor = default(System.Drawing.Color);
                if (Options.TryParseColor(Parameter[1].Value.ToString(), out newColor))
                {
                    // preparing output and changing the setting
                    string OptionName = "";
                    if (Parameter[0].Value == "1")
                    {
                        Options.EMBED_COLOR = newColor;
                        OptionName = "normal Embeds";
                    }
                    else if (Parameter[0].Value == "2")
                    {
                        Options.LOG_COLOR = newColor;
                        OptionName = "logging Embeds";
                    }
                    else if (Parameter[0].Value == "3")
                    {
                        Options.ERROR_COLOR = newColor;
                        OptionName = "error Embeds";
                    }

                    // writing to file and outputting
                    Options.WriteToFile();
                    command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Changed color of '" + OptionName + "' to: '" + Options.ColorToHex(newColor) + "'", null, Helper.DiscordHelper.EmbedColors.NormalEmbed));
                }
                else
                {
                    command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error changing Color Setting (Parsing color failed)", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                }
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error changing Color Setting", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// Handle Settings LogLevel Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="Parameter"></param>
        /// <returns></returns>
        static Task HandleSettingsCommand_LogLevel(SocketSlashCommand command, List<KeyValuePair<string, object>> Parameter)
        {
            try
            {
                int Number = -1;
                if (Int32.TryParse(Parameter[1].Value.ToString(), out Number))
                {
                    string OptionName = "";
                    // number between 1 and 3
                    if (4 > Number && Number > 0)
                    {
                        // Change setting and prepare output
                        if (Parameter[0].Value.ToString() == "1")
                        {
                            Options.LOG_LEVEL_DISCORD = Number;
                            OptionName = "Discord-Logging-Level";
                        }
                        else if (Parameter[0].Value.ToString() == "2")
                        {
                            Options.LOG_LEVEL_FILE = Number;
                            OptionName = "File-Logging-Level";
                        }

                        // output  stuff
                        string OptionValue = "";
                        switch (Number)
                        {
                            case 1:
                                OptionValue = "Info";
                                break;
                            case 2:
                                OptionValue = "Warning";
                                break;
                            case 3:
                                OptionValue = "Error";
                                break;
                        }

                        // write to file and output
                        Options.WriteToFile();
                        command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Changed '" + OptionName + "' to: '" + OptionValue + "'", null, Helper.DiscordHelper.EmbedColors.NormalEmbed));
                    }
                    else
                    {
                        command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error changing Loglevel Setting (Int not between 1 and 3)", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                    }
                }
                else
                {
                    command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error changing Loglevel Setting (Parse Int failed)", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                }
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error changing Loglevel Setting", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// Handle Settings Playlist Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="Parameter"></param>
        /// <returns></returns>
        static async Task HandleSettingsCommand_Playlist(SocketSlashCommand command, List<KeyValuePair<string, object>> Parameter)
        {
            try
            {
                string MyRegexPattern = @"^(http[s]?:\/\/open.spotify\.com\/playlist\/)?([a-zA-Z0-9]{6,})";
                Regex MySpotifyRegex = new Regex(MyRegexPattern);
                Match MySpotifyMatch = MySpotifyRegex.Match(Parameter[1].Value.ToString());


                string NewSpotifyPlaylistID = "";

                // Checking if Regex mathces and writing new ID into the variable
                if (MySpotifyMatch.Success)
                {
                    if (MySpotifyMatch.Groups.Count >= 3)
                    {
                        NewSpotifyPlaylistID = MySpotifyMatch.Groups[2].ToString();
                    }
                }
                else
                {
                    command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error changing spotify_playlist Setting (Regex doesnt hit)", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                    return;
                }


                // making sure we got a regex match earlier
                if (!String.IsNullOrEmpty(NewSpotifyPlaylistID))
                {
                    // if its the same playlist
                    if (NewSpotifyPlaylistID == Options.SPOTIFY_PLAYLIST_ID)
                    {
                        command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Thats the same fucking Playlist you dumbfuck.", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                        return;
                    }


                    List<FullTrack> OldPlaylistSongs = new List<FullTrack>();
                    // If we want to port over old Songs, save them in existing empty list
                    if (Parameter[1].Value.ToString() == "1")
                    {
                        OldPlaylistSongs = await APIs.MySpotify.GetAllPlaylistFullTracks();
                    }


                    Options.SPOTIFY_PLAYLIST_ID = NewSpotifyPlaylistID;
                    string OutputAddition = "";

                    if (Parameter[1].Value.ToString() == "1")
                    {
                        // add old playlist songs to new playlist

                        List<string> Uris = new List<string>();
                        for (int i = 0; i <= OldPlaylistSongs.Count - 1; i++)
                        {
                            Uris.Add(OldPlaylistSongs[i].Uri);
                        }
                        // convert FullTrack to UriList, make call Api, add to output
                        APIs.MySpotify.AddToPlaylist(Uris);
                        OutputAddition = ", and copied over all (" + OldPlaylistSongs.Count + ") old Songs.";
                    }

                    // Write to settings file and output
                    Options.WriteToFile();
                    command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Changed 'Playlist_ID' to: '" + NewSpotifyPlaylistID + "'" + OutputAddition, null, Helper.DiscordHelper.EmbedColors.NormalEmbed));
                }
                else
                {
                    command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error changing spotify_playlist Setting (NewSpotifyPlaylistID is null)", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                    return;
                }
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error changing spotify_playlist Setting", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
        }


        /// <summary>
        /// Handle Settings Max-Levenshtein-Distance
        /// </summary>
        /// <param name="command"></param>
        /// <param name="Parameter"></param>
        /// <returns></returns>
        static Task HandleSettingsCommand_Levenshtein(SocketSlashCommand command, List<KeyValuePair<string, object>> Parameter)
        {
            try
            {
                int value = -1;
                // See if parse was sucessfull
                if (Int32.TryParse(Parameter[0].Value.ToString(), out value))
                {
                    // if value is above null
                    if (value > 0)
                    {
                        // change setting, write to file, output
                        Options.MAX_LEVENSHTEIN_DISTANCE = value;
                        Options.WriteToFile();
                        command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Changed 'MAX_LEVENSHTEIN_DISTANCE' to: '" + value + "'", null, Helper.DiscordHelper.EmbedColors.NormalEmbed));
                    }
                    else
                    {
                        command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error changing Levenshtein Setting (Value is 0 or less)", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                    }
                }
                else
                {
                    command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error changing Levenshtein Setting (TryParse shit the bed)", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                }
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error changing Levenshtein Setting", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// Handle Settings DiscordGuildId Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="Parameter"></param>
        /// <returns></returns>
        static Task HandleSettingsCommand_DiscordGuildId(SocketSlashCommand command, List<KeyValuePair<string, object>> Parameter)
        {
            try
            {
                // Parsing input
                string input = Parameter[0].Value.ToString();
                ulong GuildID = 0;
                if (ulong.TryParse(input, out GuildID))
                {
                    // changing setting, writing to file, add output
                    Options.DISCORD_GUILD_ID = GuildID;
                    Options.WriteToFile();
                    command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Changed 'Discord_Guild_ID' to: '" + GuildID + "'", null, Helper.DiscordHelper.EmbedColors.NormalEmbed));
                }
                else
                {
                    command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error changing Discord Guild ID Setting (TryParse false)", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                }
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error changing Discord Guild ID Setting", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// Handle Settings ShowACtivityInternal Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="Parameter"></param>
        /// <returns></returns>
        static Task HandleSettingsCommand_ShowActivityInternal(SocketSlashCommand command, List<KeyValuePair<string, object>> Parameter)
        {
            try
            {
                // just normal shit, pretty basic
                if (Parameter[0].Value.ToString() == "1")
                {
                    Options.SHOW_ACTIVITY_INTERNAL = true;
                }
                else
                {
                    Options.SHOW_ACTIVITY_INTERNAL = false;
                }
                Options.WriteToFile();
                command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Changed 'show_activity_internally' to: '" + Options.SHOW_ACTIVITY_INTERNAL.ToString() + "'", null, Helper.DiscordHelper.EmbedColors.NormalEmbed));
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error changing show_activity_internal Setting", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }

            return Task.CompletedTask;
        }
    }
}
