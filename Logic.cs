﻿using Discord;
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


        /// <summary>
        /// Removes a Song if proper Reaction to our own bot message was met
        /// </summary>
        /// <param name="arg1"></param>
        /// <returns></returns>
        public static async Task RemoveSongFromReactionRequest(Cacheable<IUserMessage, ulong> arg1)
        {
            try
            {
                // grab message someone reacted to
                IUserMessage MyMessage = await arg1.DownloadAsync();

                // declare and init variables we need to remove song and output
                string SongName = "";
                string SongUri = "";

                // loop through all embeds
                foreach (Embed MyEmbed in MyMessage.Embeds)
                {
                    // loop through fields of embed
                    foreach (var EmbedField in MyEmbed.Fields)
                    {
                        // if Name of Field of Embed matches our string
                        if (EmbedField.Name == Globals.SongAddedTopline)
                        {
                            // Set Uri
                            SongUri = EmbedField.Value;
                        }

                        // if Name of Field of Embed matches our other String
                        else if (EmbedField.Name == Globals.SongAddedDescription)
                        {
                            // Set SongName
                            SongName = EmbedField.Value;
                        }
                    }
                }

                // Initiate List for Output
                List<KeyValuePair<string, string>> SongList = new List<KeyValuePair<string, string>>();

                // Add which song we removed to Output
                SongList.Add(new KeyValuePair<string, string>("Removed following Song from the Playlist, as per Reaction-Request:", SongName));

                // Remove song from Playlist
                APIs.MySpotify.RemoveFromPlaylist(SongUri);

                // output to channel
                APIs.MyDiscord.SendMessage(Helper.DiscordHelper.BuildEmbed(null, null, SongList, Helper.DiscordHelper.EmbedColors.LoggingEmbed));
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
        }




        /// <summary>
        /// Get Spotify Uri via Search from fmBOT embed.
        /// </summary>
        /// <param name="messageParam"></param>
        /// <returns></returns>
        public static async Task<string> GetFromEmbed(SocketMessage messageParam)
        {
            try
            {
                if (messageParam.Embeds != null)
                {
                    if (messageParam.Embeds.Count > 0)
                    {
                        foreach (var embed in messageParam.Embeds)
                        {
                            string FMRegex = @"^\[(.*)\]\(https:\/\/www.last.fm\/music\/.*\)\nBy \*\*(.*)\*\* \| \*(.*)\*$";
                            Match MyFMMatch = Regex.Match(embed.Description, FMRegex);

                            if (MyFMMatch.Success)
                            {
                                if (MyFMMatch.Groups.Count == 4)
                                {
                                    //Globals.DebugPrint("Song: '{0}'", MyFMMatch.Groups[1]);
                                    //Globals.DebugPrint("Artist: '{0}'", MyFMMatch.Groups[2]);
                                    //Globals.DebugPrint("Album: '{0}'", MyFMMatch.Groups[3]);
                                    return await APIs.MySpotify.GetUriFromSearch(MyFMMatch.Groups[2] + " " + MyFMMatch.Groups[1]);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
            return "";
        }


        /// <summary>
        /// Get Spotify Uri from Content
        /// </summary>
        /// <param name="Content"></param>
        /// <returns></returns>
        public static async Task<List<string>> GetFromContent(string Content)
        {
            List<string> MyUris = new List<string>();

            try
            {
                if (string.IsNullOrEmpty(Content))
                {
                    return MyUris;
                }


                // weird spotify.link/Some_ID_That_Redirects_To_proper_URL
                string anyLinkRegex = @"http[s]?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/=]*)";
                string spotifyCinniRegex = @"http[s]?:\/\/spotify\.link\/([a-zA-Z0-9]{6,})";
                string spotifyRegex = @"http[s]?:\/\/open.spotify\.com\/(embed\/)?(track|album)\/([a-zA-Z0-9]{6,})";
                string youtubeRegex = @"http[s]?:\/\/(www.)?(m.)?((youtube(-nocookie)?\.com\/((watch\?v=)|(v\/)|(embed\/)))|(youtu.be\/))[a-zA-Z0-9\-_]{5,}";
                string spotifyUriIDRegex = @"(spotify:track:)([a-zA-Z0-9]{10,30})";
                // cant match ID only because it will match other parts of other links

                // https://regex101.com/r/PAuTVh/1

                // if we have an actual Spotify Track Uri
                Regex MySpotifyUriIDRegex = new Regex(spotifyUriIDRegex);
                foreach (Match MySpotifyUriIDMatch in MySpotifyUriIDRegex.Matches(Content))
                {
                    if (MySpotifyUriIDMatch.Success)
                    {
                        if (MySpotifyUriIDMatch.Groups.Count >= 3)
                        {
                            MyUris.Add(Helper.SpotifyHelper.GetTrackUriFromTrackId(MySpotifyUriIDMatch.Groups[2].ToString()));
                        }
                    }
                }

                // Loop through all Link Matches
                Regex MyAnyLinkRegex = new Regex(anyLinkRegex);
                foreach (Match MyAnyLinkMatch in MyAnyLinkRegex.Matches(Content))
                {
                    if (MyAnyLinkMatch.Success)
                    {
                        // Set Content Variable to current link match
                        Content = MyAnyLinkMatch.Groups[0].Value;

                        // Spotify "Cinni" type Url
                        Regex MySpotifyCinniRegex = new Regex(spotifyCinniRegex);
                        foreach (Match MySpotifyCinniMatch in MySpotifyCinniRegex.Matches(Content))
                        {
                            if (MySpotifyCinniMatch.Success)
                            {
                                string tmp = await Helper.NetworkHelper.GetFinalRedirectUrlFromUrl(MySpotifyCinniMatch.Groups[0].ToString());
                                Content = Content + " " + tmp;
                            }
                        }

                        // Spotify Track or Album link
                        Regex MySpotifyRegex = new Regex(spotifyRegex);
                        foreach (Match MySpotifyMatch in MySpotifyRegex.Matches(Content))
                        {
                            if (MySpotifyMatch.Success)
                            {
                                if (MySpotifyMatch.Groups.Count >= 4)
                                {
                                    string Id = MySpotifyMatch.Groups[3].ToString();
                                    string Type = MySpotifyMatch.Groups[2].ToString().ToLower();

                                    // if Track Uri
                                    if (Type == "track")
                                    {
                                        // return with some string before
                                        MyUris.Add(Helper.SpotifyHelper.GetTrackUriFromTrackId(Id));
                                    }
                                    // if album
                                    else if (Type == "album")
                                    {
                                        // method that checks if album only has one track etc.
                                        string tmp = await APIs.MySpotify.GetTrackUriFromAlbum(Id);
                                        if (!String.IsNullOrWhiteSpace(tmp))
                                        {
                                            MyUris.Add(tmp);
                                        }
                                    }
                                }
                            }
                        }

                        // Youtube link
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

                                    // Call youtube API
                                    tmp = await APIs.MyYoutube.GetSearchStringFromYoutubeID(tmp);

                                    MyUris.Add(await APIs.MySpotify.GetUriFromSearch(tmp));
                                }
                            }
                        }
                    }
                }
                return MyUris;
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
                return MyUris;
            }
        }





        /// <summary>
        /// Processing of a messageParam
        /// </summary>
        /// <param name="messageParam"></param>
        /// <param name="prevMessageParam"></param>
        /// <returns></returns>
        public static async Task ProcessPotentialSong(SocketMessage messageParam, SocketMessage prevMessageParam)
        {
            try
            {
                string authorr = messageParam.Author.Username;
                if (string.IsNullOrEmpty(authorr))
                {
                    authorr = "FMBot";
                }

                Globals.DebugPrint("Message by '" + authorr + "#" + messageParam.Author.Discriminator + "' in '" + messageParam.Channel + "': '" + messageParam.Content + "'");

                List<string> MyUris = new List<string>();

                // if prevMessage is null
                if (prevMessageParam == null)
                {
                    // and the message is from FM Bot
                    if (messageParam.Author.Id == Globals.FMBotID)
                    {
                        // do nothing
                        return;
                    }
                }

                // set UserID to 0
                ulong UserID = 0;

                // If curr message author is bot
                if (messageParam.Author.Id == Globals.FMBotID)
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


                // Do the Logic to get the Uris
                string tmp = await GetFromEmbed(messageParam);
                if (!String.IsNullOrEmpty(tmp))
                {
                    MyUris.Add(tmp);
                }
                foreach (string tmpUri in await GetFromContent(messageParam.Content))
                {
                    MyUris.Add(tmpUri);
                }

                AddSongs(MyUris, UserID, messageParam);
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
        }








        /// <summary>
        /// Add Songs (list of Uri)
        /// </summary>
        /// <param name="MyUris"></param>
        /// <param name="UserID"></param>
        /// <param name="messageParam"></param>
        /// <returns></returns>
        public static async Task AddSongs(List<string> MyUris, ulong UserID, SocketMessage messageParam = null)
        {
            try
            {
                string tmpLink = "Unknown_Message_Link";


                if (messageParam != null)
                {
                    tmpLink = @"https://discord.com/channels/" + Options.DISCORD_GUILD_ID + "/" + messageParam.Channel.Id + "/" + messageParam.Id;
                }


                // List of Real Uris
                List<string> MyUrisInput = MyUris;
                Helper.ListHelper.RemoveNullEmptyWhitespaceDuplicateStringList(ref MyUrisInput);
                // ideally we want to use Helper.ListHelper.CanContinueWithList(ref MyUris) and do with that...

                List<FullTrack> FullTracksInPlaylist = await APIs.MySpotify.GetAllPlaylistFullTracks();
                List<string> UrisToAdd = new List<string>();
                List<string> UrisAddable = new List<string>();
                List<string> UrisNotInLimit = new List<string>();
                List<string> AlreadyExistingUris = new List<string>();

                //loop through all Uris we want to add
                for (int i = 0; i <= MyUrisInput.Count - 1; i++)
                {
                    // Remove Songs that are already in playlist
                    // add to UrisToAdd and DoubledUris

                    bool dontAdd = false;

                    // loop through all Tracks
                    for (int j = 0; j <= FullTracksInPlaylist.Count - 1; j++)
                    {
                        // if Uri matches
                        if (MyUrisInput[i] == FullTracksInPlaylist[j].Uri)
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
                        UrisAddable.Add(MyUrisInput[i]);
                    }
                    else
                    {
                        AlreadyExistingUris.Add(MyUrisInput[i]);
                    }
                }



                // if an actual User and not added Via Command
                if (UserID > 0)
                {
                    // get amount of songs user is able to add
                    int NumberOfSongsUserCanAdd = UserSongs.SongsUserCanAdd(UserID);

                    // if UrisToAdd has more than the amount of songs we can add
                    if (UrisAddable.Count > NumberOfSongsUserCanAdd)
                    {
                        // fill UrisNotInLimit and UrisToAdd

                        // array with 5 elements, index 0 to 4, 3 songs can be added

                        //                                    3, 5-3 = 2 >>> 3, 2, also index 3 und 4, also das 4 und 5 element
                        // correct and tested
                        UrisNotInLimit = UrisAddable.GetRange(NumberOfSongsUserCanAdd, UrisAddable.Count - NumberOfSongsUserCanAdd);
                        UrisToAdd = UrisAddable.GetRange(0, NumberOfSongsUserCanAdd);
                    }
                    else
                    {
                        UrisToAdd = UrisAddable;
                    }

                    // Add count to UserSong object
                    UserSongs.AddUserSongs(UserID, UrisToAdd.Count);
                }
                else
                {
                    UrisToAdd = UrisAddable;
                }

                List<FullTrack> removedSongs = new List<FullTrack>();


                //Console.WriteLine();
                //Console.WriteLine();
                //Console.WriteLine();
                //Console.WriteLine("Add Songs:");
                //Console.WriteLine("MyUrisInput.Count: " + MyUrisInput.Count);
                //for (int i = 0; i < MyUrisInput.Count; i++)
                //{
                //    Console.WriteLine(" --- MyUrisInput[{0}]: {1}", i, MyUrisInput[i]);
                //}
                //Console.WriteLine("UserID: " + UserID);
                //Console.WriteLine("UrisToAdd: " + UrisToAdd.Count);
                //Console.WriteLine("UrisNotInLimit: " + UrisNotInLimit.Count);
                //Console.WriteLine("AlreadyExistingUris: " + AlreadyExistingUris.Count);
                //Console.WriteLine();
                //Console.WriteLine();
                //Console.WriteLine();


                // Can we continue with our List
                if (Helper.ListHelper.CanContinueWithList(ref UrisToAdd))
                {
                    // Add our Songs to playlist
                    await APIs.MySpotify.AddToPlaylist(UrisToAdd);
                    removedSongs = await APIs.MySpotify.KeepSongLimit();
                }
                else
                {
                    int AlreadyExisting = AlreadyExistingUris.Count;
                    int NotInLimit = UrisNotInLimit.Count;

                    // Dont output to channel...could be normal text message
                    Globals.DebugPrint("No songs to add from that message (" + tmpLink + "). For Context: " + AlreadyExisting + " Songs were aready present in the playlist and " + NotInLimit + " songs were not added because the User has reached their daily limit.");
                }


                // this is here and not "if (Helper.ListHelper.CanContinueWithList(ref UrisToAdd))" above
                // because this should be shown even if we can not add Uris to playlist
                if (Options.SHOW_ACTIVITY_INTERNAL)
                {
                    // ADDED TRACKS

                    // Get information
                    List<FullTrack> AddedTracks = await APIs.MySpotify.GetTracksReponse(UrisToAdd);

                    // loop through AddedTracks
                    for (int i = 0; i <= AddedTracks.Count - 1; i++)
                    {
                        List<KeyValuePair<string, string>> tmp = new List<KeyValuePair<string, string>>();

                        // if messageparam is NOT null, so we add organically and not via command
                        if (!(messageParam == null))
                        {
                            // if we have an User (difference only in adding User to output)
                            if (UserID > 0)
                            {
                                SocketGuildUser SGU = APIs.MyDiscord.MyDiscordClient.GetGuild(Options.DISCORD_GUILD_ID).GetUser(UserID);
                                tmp.Add(new KeyValuePair<string, string>("User: " + Helper.DiscordHelper.GetUserTextFromSGU(SGU), "Link: " + tmpLink));
                            }
                            // If we dont
                            else
                            {
                                tmp.Add(new KeyValuePair<string, string>("Link:", tmpLink));
                            }
                        }


                        // Add to output
                        tmp.Add(new KeyValuePair<string, string>(Globals.SongAddedDescription, Helper.SpotifyHelper.GetTrackString(AddedTracks[i])));
                        tmp.Add(new KeyValuePair<string, string>(Globals.SongAddedTopline, AddedTracks[i].Uri));

                        // output
                        Discord.Rest.RestUserMessage RUM = await APIs.MyDiscord.SendMessage(Helper.DiscordHelper.BuildEmbed(null, null, tmp, Helper.DiscordHelper.EmbedColors.LoggingEmbed));

                        // Add reaction to the message
                        if (RUM != null)
                        {
                            await RUM.AddReactionAsync(Globals.MyReactionEmote);
                        }
                    }





                    // If we have songs we could not add because they were in the playlist already
                    if (AlreadyExistingUris.Count > 0)
                    {
                        List<FullTrack> TResponseDoubled = await APIs.MySpotify.GetTracksReponse(AlreadyExistingUris);
                        string sth = "Unfortunately were not able to add the following Songs,\nfrom: " + tmpLink + " \nbecause they are in the Playlist already";
                        List<KeyValuePair<string, string>> outputlist = new List<KeyValuePair<string, string>>();
                        for (int i = 0; i <= TResponseDoubled.Count - 1; i++)
                        {
                            outputlist.Add(new KeyValuePair<string, string>(Helper.SpotifyHelper.GetTrackString(TResponseDoubled[i]), TResponseDoubled[i].Uri));
                        }
                        APIs.MyDiscord.SendMessage(Helper.DiscordHelper.BuildEmbed(null, sth, outputlist, Helper.DiscordHelper.EmbedColors.LoggingEmbed));
                    }



                    // Limit
                    // If the UserID is someone
                    if (UserID > 0)
                    {
                        // If we actually have Uris
                        if (UrisNotInLimit.Count > 0)
                        {
                            // Output all songs we could not add due to daily limit
                            List<FullTrack> TResponseNotAdding = await APIs.MySpotify.GetTracksReponse(UrisNotInLimit);
                            SocketGuildUser SGU = APIs.MyDiscord.MyDiscordClient.GetGuild(Options.DISCORD_GUILD_ID).GetUser(UserID);
                            string sth = "Unfortunately were not able to add the following Songs,\nfrom: " + tmpLink + " \nbecause the User: " + Helper.DiscordHelper.GetUserTextFromSGU(SGU) + "\nhas reached their daily limit of " + Options.USER_DAILY_LIMIT + " Songs.";
                            List<KeyValuePair<string, string>> outputlist = new List<KeyValuePair<string, string>>();
                            for (int i = 0; i <= TResponseNotAdding.Count - 1; i++)
                            {
                                outputlist.Add(new KeyValuePair<string, string>(Helper.SpotifyHelper.GetTrackString(TResponseNotAdding[i]), TResponseNotAdding[i].Uri));
                            }
                            APIs.MyDiscord.SendMessage(Helper.DiscordHelper.BuildEmbed(null, sth, outputlist, Helper.DiscordHelper.EmbedColors.LoggingEmbed));
                        }
                    }



                    // Removed Songs for Playlist Number of Songs limit
                    if (removedSongs.Count > 0)
                    {
                        List<KeyValuePair<string, string>> OutputList = new List<KeyValuePair<string, string>>();
                        string Output = "";
                        for (int i = 0; i <= removedSongs.Count - 1; i++)
                        {
                            Output += Helper.SpotifyHelper.GetTrackString(removedSongs[i]);
                            if (i < removedSongs.Count - 1)
                            {
                                Output += "\n";
                            }
                        }

                        OutputList.Add(new KeyValuePair<string, string>("In Order to keep the Song limit we had to remove the following Songs:", Output));
                        APIs.MyDiscord.SendMessage(Helper.DiscordHelper.BuildEmbed(null, null, OutputList, Helper.DiscordHelper.EmbedColors.LoggingEmbed));
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
        }
    }
}
