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
        /// Handle Songs Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="UserIsAdmin"></param>
        /// <returns></returns>
        static async Task HandleCommand_Songs(SocketSlashCommand command, bool UserIsAdmin)
        {
            var fieldName = command.Data.Options.First().Name;
            switch (fieldName)
            {
                case "list":
                    {
                        HandleCommand_Songs_List(command, UserIsAdmin);
                    }
                    break;
                case "add":
                    {
                        HandleCommand_Songs_Add(command, UserIsAdmin);
                    }
                    break;
                case "remove-at-index":
                    {
                        HandleCommand_Songs_RemoveAtIndex(command, UserIsAdmin);
                    }
                    break;
                case "remove-song":
                    {
                        HandleCommand_Songs_RemoveSong(command, UserIsAdmin);
                    }
                    break;
            }
        }


        /// <summary>
        /// Handle Songs List Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="UserIsAdmin"></param>
        /// <returns></returns>
        static async Task HandleCommand_Songs_List(SocketSlashCommand command, bool UserIsAdmin)
        {
            List<FullTrack> FullTracks = await APIs.MySpotify.GetAllPlaylistFullTracks();
            string tmp = "";

            List<KeyValuePair<string, string>> MyList = new List<KeyValuePair<string, string>>();

            if (FullTracks.Count == 0)
            {
                MyList.Add(new KeyValuePair<string, string>("Empty Playlist", "No Songs in Playlist."));
                command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, null, MyList, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                return;
            }

            try
            {
                // Loop through all songs
                for (int i = 0; i <= FullTracks.Count - 1; i++)
                {
                    // add string to output
                    tmp = tmp + (i + 1) + ") " + Helper.SpotifyHelper.GetArtistString(FullTracks[i], ", ") + " " + FullTracks[i].Name + "\n";

                    // if we are at the last track
                    if (i == FullTracks.Count - 1)
                    {
                        // trim new line
                        tmp.TrimEnd('\n');

                        //if we have something in the output list already
                        if (MyList.Count > 0)
                        {
                            // add seperation with new songs
                            MyList.Add(new KeyValuePair<string, string>("------------------------", tmp));

                        }
                        // if we dont have something in output already
                        else
                        {
                            // add heading line with songs
                            MyList.Add(new KeyValuePair<string, string>("Alle Songs der Playlist:", tmp));
                        }

                        // break out of loop so we dont finish this loop iteration
                        break;
                    }

                    // if we are at the 10th, 20th, 30th etc. song
                    if ((i + 1) % 10 == 0)
                    {
                        // trim newline at end
                        tmp.TrimEnd('\n');

                        // if its the first time we get here
                        if (i + 1 == 10)
                        {
                            MyList.Add(new KeyValuePair<string, string>("Alle Songs der Playlist:", tmp));
                        }
                        // if its a second, third... "chapter"
                        else
                        {
                            MyList.Add(new KeyValuePair<string, string>("------------------------", tmp));
                        }
                        tmp = "";
                    }
                }
                // output
                command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, null, MyList, Helper.DiscordHelper.EmbedColors.NormalEmbed));
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error listing all Songs", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
        }

        /// <summary>
        /// Handle Songs Add Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="UserIsAdmin"></param>
        /// <returns></returns>
        static async Task HandleCommand_Songs_Add(SocketSlashCommand command, bool UserIsAdmin)
        {
            if (!UserIsAdmin)
            {
                MissingPermissions(command);
                return;
            }


            try
            {
                string input = command.Data.Options.First().Options.First().Value.ToString();
                List<string> Uris = await Logic.GetFromContent(input);
                // Get Input, make Uri list with Links, see if we can continue
                if (Helper.ListHelper.CanContinueWithList(ref Uris))
                {
                    // init and declare variables
                    List<FullTrack> FullTracks = await APIs.MySpotify.GetTracksReponse(Uris);
                    List<KeyValuePair<string, string>> Output = new List<KeyValuePair<string, string>>();
                    string topline = "Tried adding " + FullTracks.Count + " song(s):";
                    string description = "";
                    // Loop through Track Information
                    for (int i = 0; i <= FullTracks.Count - 1; i++)
                    {
                        // add to output
                        description += Helper.SpotifyHelper.GetTrackString(FullTracks[i]);
                        description += '\n';
                    }
                    description = description.TrimEnd('\n');
                    Output.Add(new KeyValuePair<string, string>(topline, description));
                    
                    // Actually adding songs
                    Logic.AddSongs(Uris, 0);
                    command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, null, Output, Helper.DiscordHelper.EmbedColors.NormalEmbed));
                }
                else
                {
                    command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "No songs found", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                }
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error adding Songs", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
        }


        /// <summary>
        /// Handle Songs RemoveAtIndex Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="UserIsAdmin"></param>
        /// <returns></returns>
        static async Task HandleCommand_Songs_RemoveAtIndex(SocketSlashCommand command, bool UserIsAdmin)
        {
            if (!UserIsAdmin)
            {
                MissingPermissions(command);
                return;
            }

            try
            {
                // try parsing the input
                string input = command.Data.Options.First().Options.First().Value.ToString();
                int tryy = -1;
                if (Int32.TryParse(input, out tryy))
                {
                    // "correct" input (number) to index.
                    int Index = tryy - 1;
                    List<FullTrack> FullTracks = await APIs.MySpotify.GetAllPlaylistFullTracks();

                    // if index is within range
                    if (FullTracks.Count -1 >= Index && Index >= 0)
                    {
                        // Get Uri from track, remove it
                        string uri = FullTracks[Index].Uri;
                        APIs.MySpotify.RemoveFromPlaylist(uri);
                        command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Tried removing song at index: " + input, null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                    }
                    else
                    {
                        command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error Removing SongAtIndex (Argument out of Bounds)", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                    }
                }
                else
                {
                    command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error Removing SongAtIndex (TryParse failed)", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                }
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error Removing SongAtIndex", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
        }


        /// <summary>
        /// Handle Songs RemoveSong Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="UserIsAdmin"></param>
        /// <returns></returns>
        static async Task HandleCommand_Songs_RemoveSong(SocketSlashCommand command, bool UserIsAdmin)
        {
            if (!UserIsAdmin)
            {
                MissingPermissions(command);
                return;
            }

            try
            {
                // Get Input, get Uris from it
                string input = command.Data.Options.First().Options.First().Value.ToString();
                List<string> Uris = await Logic.GetFromContent(input);

                // if Uri list is good
                if (Helper.ListHelper.CanContinueWithList(ref Uris))
                {
                    // Start Building output
                    List<FullTrack> FullTracks = await APIs.MySpotify.GetTracksReponse(Uris);
                    List<KeyValuePair<string,string>> Output = new List<KeyValuePair<string,string>>();
                    string topline = "Tried removing " + FullTracks.Count + " song(s):";
                    string description = "";
                    for (int i = 0; i <= FullTracks.Count -1; i++)
                    {
                        description += Helper.SpotifyHelper.GetTrackString(FullTracks[i]);
                        description += '\n';
                    }
                    description = description.TrimEnd('\n');
                    Output.Add(new KeyValuePair<string, string>(topline, description ));
                    
                    // Actual call to API
                    APIs.MySpotify.RemoveFromPlaylist(Uris);
                    command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, null, Output, Helper.DiscordHelper.EmbedColors.NormalEmbed));
                }
                else
                {
                    command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "No songs found", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                }
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Helper.DiscordHelper.BuildEmbed(command, "Error removing Songs", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
        }


    }
}
