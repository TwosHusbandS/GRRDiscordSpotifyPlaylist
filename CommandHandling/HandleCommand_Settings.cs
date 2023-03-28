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
                    case "spotify-id":
                        HandleSettingsCommand_Playlist(command, parameter);
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
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Error processing Settings command", null, Globals.EmbedColors.ErrorEmbed));
            }
            return Task.CompletedTask;

        }

        static async Task HandleSettings_Show(SocketSlashCommand command)
        {
            List<KeyValuePair<string, string>> myList = new List<KeyValuePair<string, string>>();
            myList.Add(new KeyValuePair<string, string>("Showing all Settings", Options.CommandListOutput()));
            command.RespondAsync(embed: Globals.BuildEmbed(command, null, myList, Globals.EmbedColors.NormalEmbed));
        }

        static async Task HandleSettings_ReRead(SocketSlashCommand command)
        {
            Options.ReadFromFile();
            List<KeyValuePair<string, string>> myList = new List<KeyValuePair<string, string>>();
            string Description = "Sucessfully Re-Read all Settings from file.\nNew Settings below:\n\n";
            myList.Add(new KeyValuePair<string, string>("Showing all (new) Settings", Options.CommandListOutput()));
            command.RespondAsync(embed: Globals.BuildEmbed(command, Description, myList, Globals.EmbedColors.NormalEmbed));
        }

        static Task HandleSettings_Limit(SocketSlashCommand command, List<KeyValuePair<string, object>> Parameter)
        {
            try
            {
                int Number = 0;
                if (Int32.TryParse(Parameter[1].Value.ToString(), out Number))
                {
                    string OptionName = "";
                    if (Parameter[0].Value.ToString() == "1")
                    {
                        Options.SPOTIFY_PLAYLIST_NUMBER_OF_SONGS = Number;
                        OptionName = "playlist_amount_of_songs";
                    }
                    else if (Parameter[0].Value.ToString() == "2")
                    {
                        Options.USER_DAILY_LIMIT = Number;
                        OptionName = "user_daily_limit";
                    }
                    Options.WriteToFile();
                    command.RespondAsync(embed: Globals.BuildEmbed(command, "Changed '" + OptionName + "' to: '" + Number + "'", null, Globals.EmbedColors.NormalEmbed));
                }
                else
                {
                    command.RespondAsync(embed: Globals.BuildEmbed(command, "Error changing Limit Setting (TryParse)", null, Globals.EmbedColors.ErrorEmbed));
                }
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Error changing Limit Setting", null, Globals.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }

            return Task.CompletedTask;
        }


        static Task HandleSettingsCommand_Channel(SocketSlashCommand command, List<KeyValuePair<string, object>> Parameter)
        {
            try
            {
                SocketGuildChannel channel = (SocketGuildChannel)Parameter[1].Value;
                string OptionName = "";
                if (Parameter[0].Value == "1")
                {
                    Options.DISCORD_PUBLIC_CHANNEL = channel.Id;
                    OptionName = "public-channel";
                }
                else if (Parameter[0].Value == "2")
                {
                    Options.DISCORD_INTERNAL_CHANNEL = channel.Id;
                    OptionName = "internal-channel";
                }
                Options.WriteToFile();
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Changed '" + OptionName + "' to: '#" + channel.Name + "'", null, Globals.EmbedColors.NormalEmbed));
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Error changing Channel Setting", null, Globals.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
            return Task.CompletedTask;
        }

        static Task HandleSettingsCommand_Color(SocketSlashCommand command, List<KeyValuePair<string, object>> Parameter)
        {
            try
            {
                System.Drawing.Color newColor = default(System.Drawing.Color);

                if (Options.TryParseColor(Parameter[1].Value.ToString(), out newColor))
                {
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
                    Options.WriteToFile();
                    command.RespondAsync(embed: Globals.BuildEmbed(command, "Changed color of '" + OptionName + "' to: '" + Options.ColorToHex(newColor) + "'", null, Globals.EmbedColors.NormalEmbed));
                }
                else
                {
                    command.RespondAsync(embed: Globals.BuildEmbed(command, "Error changing Color Setting (Parsing color failed)", null, Globals.EmbedColors.ErrorEmbed));
                }
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Error changing Color Setting", null, Globals.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
            return Task.CompletedTask;
        }


        static Task HandleSettingsCommand_LogLevel(SocketSlashCommand command, List<KeyValuePair<string, object>> Parameter)
        {
            try
            {
                int Number = (int)Parameter[1].Value;
                string OptionName = "";
                if (4 > Number && Number > 0)
                {
                    if (Parameter[0].Value == "1")
                    {
                        Options.LOG_LEVEL_DISCORD = Number;
                        OptionName = "Discord-Logging-Level";
                    }
                    else if (Parameter[0].Value == "2")
                    {
                        Options.LOG_LEVEL_FILE = Number;
                        OptionName = "File-Logging-Level";
                    }
                    Options.WriteToFile();

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

                    command.RespondAsync(embed: Globals.BuildEmbed(command, "Changed '" + OptionName + "' to: '" + OptionValue + "'", null, Globals.EmbedColors.NormalEmbed));
                }
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Error changing Loglevel Setting", null, Globals.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
            return Task.CompletedTask;
        }

        static async Task HandleSettingsCommand_Playlist(SocketSlashCommand command, List<KeyValuePair<string, object>> Parameter)
        {
            try
            {

                string MyRegexPattern = @"^(http[s]?:\/\/open.spotify\.com\/playlist\/)?([a-zA-Z0-9]{6,})";
                Regex MySpotifyRegex = new Regex(MyRegexPattern);
                Match MySpotifyMatch = MySpotifyRegex.Match(Parameter[2].Value.ToString());

                string NewSpotifyPlaylistID = "";

                if (MySpotifyMatch.Success)
                {
                    if (MySpotifyMatch.Groups.Count >= 3)
                    {
                        NewSpotifyPlaylistID = MySpotifyMatch.Groups[2].ToString();
                    }
                }
                else
                {
                    command.RespondAsync(embed: Globals.BuildEmbed(command, "Error changing spotify_playlist Setting (Regex doesnt hit)", null, Globals.EmbedColors.ErrorEmbed));
                    return;
                }

                if (!String.IsNullOrEmpty(NewSpotifyPlaylistID))
                {
                    if (NewSpotifyPlaylistID == Options.SPOTIFY_PLAYLIST_ID)
                    {
                        command.RespondAsync(embed: Globals.BuildEmbed(command, "Thats the same fucking Playlist you dumbfuck.", null, Globals.EmbedColors.ErrorEmbed));
                        return;
                    }

                    List<FullTrack> FullTracks = new List<FullTrack>();
                    string OutputAddition = "";

                    if (Parameter[1].Value.ToString() == "1")
                    {
                        FullTracks = await APIs.MySpotify.GetAllPlaylistFullTracks();
                    }

                    Options.SPOTIFY_PLAYLIST_ID = NewSpotifyPlaylistID;

                    if (Parameter[1].Value.ToString() == "1")
                    {
                        // add to new playlist
                        List<string> Uris = new List<string>();
                        for (int i = 0; i <= FullTracks.Count - 1; i++)
                        {
                            Uris.Add(FullTracks[i].Uri);
                        }
                        await APIs.MySpotify.AddToPlaylist(Uris);
                        OutputAddition = ", and copied over all (" + FullTracks.Count + ") old Songs.";
                    }

                    Options.WriteToFile();

                    command.RespondAsync(embed: Globals.BuildEmbed(command, "Changed 'Playlist_ID' to: '" + NewSpotifyPlaylistID + "'" + OutputAddition, null, Globals.EmbedColors.NormalEmbed));
                }
                else
                {
                    command.RespondAsync(embed: Globals.BuildEmbed(command, "Error changing spotify_playlist Setting (NewSpotifyPlaylistID is null)", null, Globals.EmbedColors.ErrorEmbed));
                    return;
                }
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Error changing spotify_playlist Setting", null, Globals.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
        }


        static Task HandleSettingsCommand_DiscordGuildId(SocketSlashCommand command, List<KeyValuePair<string, object>> Parameter)
        {
            try
            {
                string input = Parameter[1].Value.ToString();
                ulong GuildID = 0;
                if (ulong.TryParse(input, out GuildID))
                {
                    Options.DISCORD_GUILD_ID = GuildID;
                    Options.WriteToFile();
                    command.RespondAsync(embed: Globals.BuildEmbed(command, "Changed 'Discord_Guild_ID' to: '" + GuildID + "'", null, Globals.EmbedColors.NormalEmbed));
                }
                else
                {
                    command.RespondAsync(embed: Globals.BuildEmbed(command, "Error changing Discord Guild ID Setting (TryParse false)", null, Globals.EmbedColors.ErrorEmbed));
                }
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Error changing Discord Guild ID Setting", null, Globals.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
            return Task.CompletedTask;
        }

        static Task HandleSettingsCommand_ShowActivityInternal(SocketSlashCommand command, List<KeyValuePair<string, object>> Parameter)
        {
            try
            {
                if (Parameter[1].Value.ToString() == "1")
                {
                    Options.SHOW_ACTIVITY_INTERNAL = true;
                }
                else
                {
                    Options.SHOW_ACTIVITY_INTERNAL = false;
                }
                Options.WriteToFile();
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Changed 'show_activity_internally' to: '" + Options.SHOW_ACTIVITY_INTERNAL.ToString() + "'", null, Globals.EmbedColors.NormalEmbed));
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Error changing show_activity_internal Setting", null, Globals.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }

            return Task.CompletedTask;
        }
    }
}
