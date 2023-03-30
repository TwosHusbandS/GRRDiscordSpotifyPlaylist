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

            /*
            var CommandSongs = new Discord.SlashCommandBuilder()
            .WithName("songs")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("list")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("add")
                .AddOption("link", ApplicationCommandOptionType.String, "link to the song you want to add", isRequired: true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("remove")
                .AddOption("index", ApplicationCommandOptionType.Integer, "Number of the song you want to remove", isRequired: true));
            */
        }


        /// ////////////////////////////////////////////////////////////////////// ///
        /// /////                                                            ///// ///
        /// ////////////////////////////////////////////////////////////////////// ///



        static async Task HandleCommand_Songs_List(SocketSlashCommand command, bool UserIsAdmin)
        {
            List<FullTrack> FullTracks = await APIs.MySpotify.GetAllPlaylistFullTracks();
            string tmp = "";

            List<KeyValuePair<string, string>> MyList = new List<KeyValuePair<string, string>>();

            if (FullTracks.Count == 0)
            {
                MyList.Add(new KeyValuePair<string, string>("Empty Playlist", "No Songs in Playlist."));
                command.RespondAsync(embed: Globals.BuildEmbed(command, null, MyList, Globals.EmbedColors.ErrorEmbed));
                return;
            }

            try
            {
                for (int i = 0; i <= FullTracks.Count - 1; i++)
                {
                    tmp = tmp + (i + 1) + ") " + APIs.MySpotify.GetArtistString(FullTracks[i], ", ") + " " + FullTracks[i].Name + "\n";

                    if (i == FullTracks.Count - 1)
                    {
                        tmp.TrimEnd('\n');
                        if (MyList.Count > 0)
                        {
                            MyList.Add(new KeyValuePair<string, string>("------------------------", tmp));

                        }
                        else
                        {
                            MyList.Add(new KeyValuePair<string, string>("Alle Songs der Playlist:", tmp));
                        }
                        break;
                    }

                    if ((i + 1) % 10 == 0)
                    {
                        tmp.TrimEnd('\n');
                        if (i + 1 == 10)
                        {
                            MyList.Add(new KeyValuePair<string, string>("Alle Songs der Playlist:", tmp));
                        }
                        else
                        {
                            MyList.Add(new KeyValuePair<string, string>("------------------------", tmp));
                        }
                        tmp = "";
                    }
                }
                command.RespondAsync(embed: Globals.BuildEmbed(command, null, MyList, Globals.EmbedColors.NormalEmbed));
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Error listing all Songs", null, Globals.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
        }

        static async Task HandleCommand_Songs_Add(SocketSlashCommand command, bool UserIsAdmin)
        {
            if (!UserIsAdmin)
            {
                MissingPermissions(command);
                return;
            }


            try
            {
                string rtrn = command.Data.Options.First().Options.First().Value.ToString();
                List<string> Uris = await Logic.GetFromContent(rtrn);
                Uris = Globals.RemoveEmptyAndDoubles(Uris);
                if (Uris.Count > 0)
                {
                    List<FullTrack> FullTracks = await APIs.MySpotify.GetTracksReponse(Uris);
                    List<KeyValuePair<string, string>> Output = new List<KeyValuePair<string, string>>();
                    string topline = "Tried adding " + FullTracks.Count + " song(s):";
                    string description = "";
                    for (int i = 0; i <= FullTracks.Count - 1; i++)
                    {
                        description += APIs.MySpotify.GetTrackString(FullTracks[i]);
                        description += '\n';
                    }
                    description = description.TrimEnd('\n');
                    Output.Add(new KeyValuePair<string, string>(topline, description));
                    Logic.AddSongs(Uris, 0);
                    command.RespondAsync(embed: Globals.BuildEmbed(command, null, Output, Globals.EmbedColors.NormalEmbed));
                }
                else
                {
                    command.RespondAsync(embed: Globals.BuildEmbed(command, "No songs found", null, Globals.EmbedColors.ErrorEmbed));
                }
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Error adding Songs", null, Globals.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
        }

        static async Task HandleCommand_Songs_RemoveAtIndex(SocketSlashCommand command, bool UserIsAdmin)
        {
            if (!UserIsAdmin)
            {
                MissingPermissions(command);
                return;
            }

            try
            {
                string rtrn = command.Data.Options.First().Options.First().Value.ToString();
                int tryy = -1;
                Int32.TryParse(rtrn, out tryy);
                if (tryy != -1)
                {
                    int Index = tryy - 1;
                    List<FullTrack> FullTracks = await APIs.MySpotify.GetAllPlaylistFullTracks();
                    if (FullTracks.Count >= Index)
                    {
                        string uri = FullTracks[Index].Uri;
                        APIs.MySpotify.RemoveFromPlaylist(uri);
                        command.RespondAsync(embed: Globals.BuildEmbed(command, "Tried removing song at index: " + rtrn, null, Globals.EmbedColors.ErrorEmbed));
                    }
                }
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Error Removing SongAtIndex", null, Globals.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
        }







        static async Task HandleCommand_Songs_RemoveSong(SocketSlashCommand command, bool UserIsAdmin)
        {
            if (!UserIsAdmin)
            {
                MissingPermissions(command);
                return;
            }

            try
            {
                string rtrn = command.Data.Options.First().Options.First().Value.ToString();
                List<string> Uris = await Logic.GetFromContent(rtrn);
                Uris = Globals.RemoveEmptyAndDoubles(Uris);
                if (Uris.Count > 0)
                {
                    List<FullTrack> FullTracks = await APIs.MySpotify.GetTracksReponse(Uris);
                    List<KeyValuePair<string,string>> Output = new List<KeyValuePair<string,string>>();
                    string topline = "Tried removing " + FullTracks.Count + " song(s):";
                    string description = "";
                    for (int i = 0; i <= FullTracks.Count -1; i++)
                    {
                        description += APIs.MySpotify.GetTrackString(FullTracks[i]);
                        description += '\n';
                    }
                    description = description.TrimEnd('\n');
                    Output.Add(new KeyValuePair<string, string>(topline, description ));
                    APIs.MySpotify.RemoveFromPlaylist(Uris);
                    command.RespondAsync(embed: Globals.BuildEmbed(command, null, Output, Globals.EmbedColors.NormalEmbed));
                }
                else
                {
                    command.RespondAsync(embed: Globals.BuildEmbed(command, "No songs found", null, Globals.EmbedColors.ErrorEmbed));
                }
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Error removing Songs", null, Globals.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
        }


    }
}
