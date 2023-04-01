using Discord;
using Discord.WebSocket;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WasIchHoerePlaylist
{



    internal class Logic
    {
        private static readonly ulong FMBotID = 356268235697553409;


        public static string SpotifyPlaylist
        {
            get
            {
                return "https://open.spotify.com/playlist/" + Options.SPOTIFY_PLAYLIST_ID;
            }
        }


        public static async Task<string> GetFromEmbed(SocketMessage messageParam)
        {

            foreach (var embed in messageParam.Embeds)
            {
                string FMRegex = @"^\[(.*)\]\(https:\/\/www.last.fm\/music\/.*\)\nBy \*\*(.*)\*\* \| \*(.*)\*$";
                Match MyFMMatch = Regex.Match(embed.Description, FMRegex);

                if (MyFMMatch.Success)
                {
                    if (MyFMMatch.Groups.Count == 4)
                    {
                        //Console.WriteLine("Song: '{0}'", MyFMMatch.Groups[1]);
                        //Console.WriteLine("Artist: '{0}'", MyFMMatch.Groups[2]);
                        //Console.WriteLine("Album: '{0}'", MyFMMatch.Groups[3]);
                        return await APIs.MySpotify.GetUriFromSearch(MyFMMatch.Groups[2] + " " + MyFMMatch.Groups[1]);
                    }
                }
            }
            return "";
        }

        public static async Task<List<string>> GetFromContent(string Content)
        {
            // weird spotify.link/Some_ID_That_Redirects_To_proper_URL
            string anyLinkRegex = @"http[s]?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/=]*)";
            string spotifyCinniRegex = @"http[s]?:\/\/spotify\.link\/([a-zA-Z0-9]{6,})";
            string spotifyRegex = @"http[s]?:\/\/open.spotify\.com\/(embed\/)?(track|album)\/([a-zA-Z0-9]{6,})";
            string youtubeRegex = @"http[s]?:\/\/(www.)?(m.)?((youtube(-nocookie)?\.com\/((watch\?v=)|(v\/)|(embed\/)))|(youtu.be\/))[a-zA-Z0-9\-_]{5,}";
            string spotifyUriIDRegex = @"(spotify:track:)([a-zA-Z0-9]{10,30})";
            // cant match ID only because it will match other parts of other links


            // https://regex101.com/r/PAuTVh/1

            List<string> MyUris = new List<string>();


            Regex MySpotifyUriIDRegex = new Regex(spotifyUriIDRegex);
            foreach (Match MySpotifyUriIDMatch in MySpotifyUriIDRegex.Matches(Content))
            {
                if (MySpotifyUriIDMatch.Success)
                {
                    if (MySpotifyUriIDMatch.Groups.Count >= 3)
                    {
                        MyUris.Add(APIs.MySpotify.GetTrackUriFromId(MySpotifyUriIDMatch.Groups[2].ToString()));
                    }
                }
            }



            Regex MyAnyLinkRegex = new Regex(anyLinkRegex);
            foreach (Match MyAnyLinkMatch in MyAnyLinkRegex.Matches(Content))
            {
                if (MyAnyLinkMatch.Success)
                {
                    Content = MyAnyLinkMatch.Groups[0].Value;
                    Console.WriteLine(Content);
                    Console.WriteLine();

                    Regex MySpotifyCinniRegex = new Regex(spotifyCinniRegex);
                    foreach (Match MySpotifyCinniMatch in MySpotifyCinniRegex.Matches(Content))
                    {
                        if (MySpotifyCinniMatch.Success)
                        {
                            string tmp = await Globals.UrlLengthen(MySpotifyCinniMatch.Groups[0].ToString());
                            Content = Content + " " + tmp;
                        }
                    }




                    Regex MySpotifyRegex = new Regex(spotifyRegex);
                    foreach (Match MySpotifyMatch in MySpotifyRegex.Matches(Content))
                    {
                        if (MySpotifyMatch.Success)
                        {
                            if (MySpotifyMatch.Groups.Count >= 4)
                            {
                                string Id = MySpotifyMatch.Groups[3].ToString();
                                string Type = MySpotifyMatch.Groups[2].ToString().ToLower();
                                if (Type == "track")
                                {
                                    MyUris.Add(APIs.MySpotify.GetTrackUriFromId(Id));
                                }
                                else if (Type == "album")
                                {
                                    MyUris.Add(await APIs.MySpotify.GetTrackUriFromAlbum(Id));
                                }
                            }
                        }
                    }



                    Regex MyYoutubeRegex = new Regex(youtubeRegex);
                    foreach (Match MyYoutubeMatch in MyYoutubeRegex.Matches(Content))
                    {
                        if (MyYoutubeMatch.Success)
                        {
                            if (MyYoutubeMatch.Groups.Count > 0)
                            {
                                string tmp = MyYoutubeMatch.Groups[0].ToString();
                                if (tmp.Contains('='))
                                {
                                    tmp = tmp.Substring(tmp.LastIndexOf('=') + 1);
                                }
                                else
                                {
                                    tmp = tmp.Substring(tmp.LastIndexOf('/') + 1);
                                }
                                tmp = await APIs.MyYoutube.GetTitleFromVideoId(tmp);

                                MyUris.Add(await APIs.MySpotify.GetUriFromSearch(tmp));
                            }
                        }
                    }
                }
            }




            return MyUris;
        }

        public static async Task ProcessPotentialSong(SocketMessage messageParam, SocketMessage prevMessageParam)
        {
            string PreFix = "[" + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "] - ";
            Console.WriteLine(PreFix + "Message by '{0}' in '{1}': '{2}'", messageParam.Author, messageParam.Channel, messageParam.Content);

            List<string> MyUris = new List<string>();


            // if prevMessage is null
            if (prevMessageParam == null)
            {
                // and the message is from FM Bot
                if (messageParam.Author.Id == FMBotID)
                {
                    // do nothing
                    return;
                }
            }

            // set UserID to 0
            ulong UserID = 0;

            // If curr message author is bot
            if (messageParam.Author.Id == FMBotID)
            {
                // set UserID to prev message (the one what triggered the bot. Hopefully.
                UserID = prevMessageParam.Author.Id;
            }
            else
            {
                // set UserID to the author of curr message
                UserID = messageParam.Author.Id;
            }


            // Removed cause of output further on
            // if user cant add songs
            //if (!UserSongs.CanUserAddSong(UserID))
            //{
            //    return;
            //}



            MyUris.Add(await GetFromEmbed(messageParam));
            foreach (string tmpUri in await GetFromContent(messageParam.Content))
            {
                MyUris.Add(tmpUri);
            }


            // possibilities
            // A - Link (youtube, spotify both user and fmbot with .s)
            // Check if youtube,  forward to youtube
            // check if spotify track, forward to spotify 
            // B - fm bot with .fm
            // Get Description from Embed. Example, with line break:
            //          Description: [Finale] (https://www.last.fm/music/Der+Edle+Baron/_/Finale)
            //          By **Der Edle Baron** | *Interphase *


            // need to null check prevMessageParam
            // use it to track who the stupid fmbot messages are by

            //messageParam.Author.Username

            AddSongs(MyUris, UserID, messageParam);

            return;
        }


        public static async Task AddSongs(List<string> MyUris, ulong UserID, SocketMessage messageParam = null)
        {
            List<string> MyRealUris = Globals.RemoveEmptyAndDoubles(MyUris);


            List<FullTrack> FullTracks = await APIs.MySpotify.GetAllPlaylistFullTracks();
            List<string> UrisToAdd = new List<string>();
            List<string> DoubledUris = new List<string>();

            //loop through all Uris we want to add
            for (int i = 0; i <= MyRealUris.Count - 1; i++)
            {
                bool dontAdd = false;

                // loop through all Tracks
                for (int j = 0; j <= FullTracks.Count - 1; j++)
                {
                    // if Uri matches
                    if (MyRealUris[i] == FullTracks[j].Uri)
                    {
                        // dont add = true
                        dontAdd = true;
                        break;
                    }
                }


                // if dont add is false (so we want to add
                if (!dontAdd)
                {
                    // add to Uris To Add
                    UrisToAdd.Add(MyRealUris[i]);
                }
                else
                {
                    DoubledUris.Add(MyRealUris[i]);
                }
            }


            List<string> UrisNotToAdd = new List<string>();

            // if an actual User and not added Via Command
            if (UserID > 0)
            {
                // get amount of songs user is able to add
                int NumberOfSongsUserCanAdd = UserSongs.SongsUserCanAdd(UserID);
                //NumberOfSongsUserCanAdd = 99999;

                //Console.WriteLine();
                //Console.WriteLine();
                //Console.WriteLine("Debug 2");
                //Console.WriteLine();

                //Console.WriteLine(UserID);
                //Console.WriteLine(NumberOfSongsUserCanAdd);

                // if UrisToAdd has more than the amount of songs we can add
                if (UrisToAdd.Count > NumberOfSongsUserCanAdd)
                {
                    UrisNotToAdd = UrisToAdd.GetRange(NumberOfSongsUserCanAdd, UrisToAdd.Count - NumberOfSongsUserCanAdd);
                    UrisToAdd = UrisToAdd.GetRange(0, NumberOfSongsUserCanAdd);
                }

                //Console.WriteLine(UrisToAdd.Count);
                //Console.WriteLine(UrisNotToAdd.Count);


                UserSongs.AddUserSongs(UserID, UrisToAdd.Count);
            }


            if (Globals.CanContinueWithList(ref UrisToAdd))
            {
                await APIs.MySpotify.AddToPlaylist(UrisToAdd);
                if (Options.SHOW_ACTIVITY_INTERNAL)
                {
                    List<KeyValuePair<string, string>> tmp = new List<KeyValuePair<string, string>>();

                    List<FullTrack> TResponse = await APIs.MySpotify.GetTracksReponse(UrisToAdd);
                    
                    List<FullTrack> removedSongs = await APIs.MySpotify.KeepSongLimit();

                    for (int i = 0; i <= TResponse.Count - 1; i++)
                    {
                        tmp.Clear();
                        if (!(messageParam == null))
                        {
                            string tmpLink = @"https://discord.com/channels/" + Options.DISCORD_GUILD_ID + "/" + messageParam.Channel.Id + "/" + messageParam.Id;
                            if (UserID > 0)
                            {
                                SocketGuildUser SGU = APIs.MyDiscord.MyDiscordClient.GetGuild(Options.DISCORD_GUILD_ID).GetUser(UserID);
                                tmp.Add(new KeyValuePair<string, string>("User: " + Globals.GetUserText(SGU), "Link: " + tmpLink));
                            }
                            else
                            {
                                tmp.Add(new KeyValuePair<string, string>("Link:", tmpLink));
                            }
                        }
                        tmp.Add(new KeyValuePair<string, string>(Globals.SongAddedDescription, APIs.MySpotify.GetTrackString(TResponse[i])));
                        tmp.Add(new KeyValuePair<string, string>(Globals.SongAddedTopline, TResponse[i].Uri));

                        Discord.Rest.RestUserMessage RUM = await APIs.MyDiscord.SendMessage(Globals.BuildEmbed(null, null, tmp, Globals.EmbedColors.LoggingEmbed));
                        await RUM.AddReactionAsync(Globals.MyReactionEmote);
                    }



                    if (DoubledUris.Count > 0)
                    {
                        List<FullTrack> TResponseDoubled = await APIs.MySpotify.GetTracksReponse(DoubledUris);
                        SocketGuildUser SGU = APIs.MyDiscord.MyDiscordClient.GetGuild(Options.DISCORD_GUILD_ID).GetUser(UserID);
                        string sth = "Unfortunately were not able to add the following Songs,\nbecause they are in the Playlist already";
                        List<KeyValuePair<string, string>> outputlist = new List<KeyValuePair<string, string>>();
                        for (int i = 0; i <= TResponseDoubled.Count - 1; i++)
                        {
                            outputlist.Add(new KeyValuePair<string, string>(APIs.MySpotify.GetTrackString(TResponseDoubled[i]), TResponseDoubled[i].Uri));
                        }
                        await APIs.MyDiscord.SendMessage(Globals.BuildEmbed(null, sth, outputlist, Globals.EmbedColors.LoggingEmbed));
                    }

                    if (UserID > 0)
                    {
                        if (UrisNotToAdd.Count > 0)
                        {
                            List<FullTrack> TResponseNotAdding = await APIs.MySpotify.GetTracksReponse(UrisNotToAdd);
                            SocketGuildUser SGU = APIs.MyDiscord.MyDiscordClient.GetGuild(Options.DISCORD_GUILD_ID).GetUser(UserID);
                            string sth = "Unfortunately were not able to add the following Songs,\nbecause the User: " + Globals.GetUserText(SGU) + "\nhas reached their daily limit of " + Options.USER_DAILY_LIMIT + " Songs.";
                            List<KeyValuePair<string, string>> outputlist = new List<KeyValuePair<string, string>>();
                            for (int i = 0; i <= TResponseNotAdding.Count - 1; i++)
                            {
                                outputlist.Add(new KeyValuePair<string, string>(APIs.MySpotify.GetTrackString(TResponseNotAdding[i]), TResponseNotAdding[i].Uri));
                            }
                            await APIs.MyDiscord.SendMessage(Globals.BuildEmbed(null, sth, outputlist, Globals.EmbedColors.LoggingEmbed));
                        }
                    }

                    if (removedSongs.Count > 0)
                    {
                        List<KeyValuePair<string, string>> tmp2 = new List<KeyValuePair<string, string>>();
                        string Output = "";
                        for (int i = 0; i <= removedSongs.Count - 1; i++)
                        {
                            Output += APIs.MySpotify.GetTrackString(removedSongs[i]);
                            if (i < removedSongs.Count - 1)
                            {
                                Output += "\n";
                            }
                        }

                        tmp2.Add(new KeyValuePair<string, string>("In Order to keep the Song limit we had to remove the following Songs:", Output));
                        APIs.MyDiscord.SendMessage(Globals.BuildEmbed(null, null, tmp2, Globals.EmbedColors.LoggingEmbed));
                    }
                    // send embed of removed songs
                }
            }
            else
            {
                Console.WriteLine("No songs in that message");
            }
        }
    }
}
