using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WasIchHoerePlaylist.APIs
{
    /// <summary>
    /// Class for our Discord Bot and interacting with the Discord API
    /// </summary>
    internal class MySpotify
    {
        static SpotifyClientConfig MySpotifyClientConfig;
        static SpotifyClient MySpotifyClient;
        static AutoResetEvent stopWaitHandle = new AutoResetEvent(false);
        private static EmbedIOAuthServer _server = null;

        /// <summary>
        /// Method to initialise the Spotify Bot
        /// </summary>
        /// <returns></returns>
        public static async Task Init()
        {
            _server = new EmbedIOAuthServer(new Uri("http://localhost:5001/callback"), 5001);

            // If the JSON File with Spotify API Token exists
            if (Helper.FileHandling.doesFileExist(Globals.SpotifyTokenFile))
            {
                // Refresh Token
                await RefreshAuthentication();
            }
            else
            {
                // Start Auth from Fresh
                await StartAuthentication();

                // Doing this to wait until StartAuthentication is fully complete
                // So when this Init task is awaited
                // Spotify Auth is 100% done and we can move on
                stopWaitHandle.WaitOne(); // wait for callback    
            }
            return;
        }


        #region Init And Auth


        /// <summary>
        /// Refreshes the authentication using the json from disk.
        /// </summary>
        /// <returns></returns>
        private static async Task RefreshAuthentication()
        {
            try
            {
                // stole this shit from somewhere

                var json = await Helper.FileHandling.AsyncReadContentOfFile(Globals.SpotifyTokenFile);
                var token = JsonConvert.DeserializeObject<PKCETokenResponse>(json);

                var authenticator = new PKCEAuthenticator(Options.SPOTIFY_CLIENT_ID!, token!);
                authenticator.TokenRefreshed += (sender, token) => Helper.FileHandling.WriteStringToFileOverwrite(Globals.SpotifyTokenFile, JsonConvert.SerializeObject(token));

                MySpotifyClientConfig = SpotifyClientConfig.CreateDefault()
                  .WithAuthenticator(authenticator);

                MySpotifyClient = new SpotifyClient(MySpotifyClientConfig);

                _server.Dispose();
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
        }


        /// <summary>
        /// Starts a completely new Authentication process
        /// </summary>
        /// <returns></returns>
        private static async Task StartAuthentication()
        {
            try
            {
                // this is the other half of the shit i stole. 

                var (verifier, challenge) = PKCEUtil.GenerateCodes();

                await _server.Start();
                _server.AuthorizationCodeReceived += async (sender, response) =>
                {
                    await _server.Stop();
                    PKCETokenResponse token = await new OAuthClient().RequestToken(
                      new PKCETokenRequest(Options.SPOTIFY_CLIENT_ID!, response.Code, _server.BaseUri, verifier)
                    );

                    Helper.FileHandling.WriteStringToFileOverwrite(Globals.SpotifyTokenFile, JsonConvert.SerializeObject(token));
                    await RefreshAuthentication();

                    // Setting the stopWaitHandle so the Init Task can continue.
                    stopWaitHandle.Set();
                };

                var request = new LoginRequest(_server.BaseUri, Options.SPOTIFY_CLIENT_ID!, LoginRequest.ResponseType.Code)
                {
                    CodeChallenge = challenge,
                    CodeChallengeMethod = "S256",
                    Scope = new List<string> { Scopes.UserReadEmail, Scopes.PlaylistReadPrivate, Scopes.PlaylistModifyPrivate, Scopes.PlaylistReadCollaborative, Scopes.PlaylistModifyPublic }

                };

                Uri uri = request.ToUri();
                BrowserUtil.Open(uri);

            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
        }


        #endregion



        #region Playlist Get Songs


        /// <summary>
        /// Gets all PlayableItems from a Spotify Playlist
        /// </summary>
        /// <returns></returns>
        public static async Task<List<PlaylistTrack<IPlayableItem>>> GetAllPlaylistPlayableItems(string PlaylistID)
        {
            if (String.IsNullOrWhiteSpace(PlaylistID))
            {
                PlaylistID = Options.SPOTIFY_PLAYLIST_ID;
            }

            // Return all playableItems of a playlist, order by datetime when it was added.First item was added the longest ago.
            try
            {
                Paging<PlaylistTrack<IPlayableItem>> MyTracks = await MySpotifyClient.Playlists.GetItems(PlaylistID);
                return (MyTracks.Items.OrderBy(x => x.AddedAt).ToList());
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
            return new List<PlaylistTrack<IPlayableItem>>();

            // DONT DELETE

            // This was for pagination. Aka returning really everything instead of just the first page
            // https://johnnycrazy.github.io/SpotifyAPI-NET/docs/pagination/
            //IList<PlaylistTrack<IPlayableItem>> asdf = await MySpotifyClient.PaginateAll(MyTracks);
            //return (asdf.OrderBy(x => x.AddedAt).ToList());
        }



        /// <summary>
        /// Gets all FullTracks from a Spotify Playlist
        /// </summary>
        /// <returns></returns>
        public static async Task<List<FullTrack>> GetAllPlaylistFullTracks(string PlaylistID = "")
        {
            // Init and declare list for return
            List<FullTrack> FullTracks = new List<FullTrack>();

            try
            {
                // loop through playable items, if fullTrack, add to return list
                List<PlaylistTrack<IPlayableItem>> Tracks = await GetAllPlaylistPlayableItems(PlaylistID);
                for (int i = 0; i <= Tracks.Count - 1; i++)
                {
                    if (Tracks[i].Track is FullTrack Track)
                    {
                        FullTracks.Add(Track);
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }

            return FullTracks;
        }


        #endregion


        #region Playlist Add Songs


        /// <summary>
        /// Adds a List of Uris to the Playlist
        /// </summary>
        /// <param name="Uris"></param>
        /// <returns></returns>
        public static async Task AddToPlaylist(List<string> Uris)
        {
            try
            {
                PlaylistAddItemsRequest PAIR = new PlaylistAddItemsRequest(Uris);
                await MySpotify.MySpotifyClient.Playlists.AddItems(Options.SPOTIFY_PLAYLIST_ID, PAIR);
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
        }



        /// <summary>
        /// Adds a Single URI to the Playlist
        /// </summary>
        /// <param name="Uri"></param>
        /// <returns></returns>
        public static async Task AddToPlaylist(string Uri)
        {
            // Calls overloaded Method
            List<string> UriList = new List<string>();
            UriList.Add(Uri);
            await AddToPlaylist(UriList);
        }

        #endregion



        #region Playlist Remove Songs



        /// <summary>
        /// Removes a single URI from Playlist
        /// </summary>
        /// <param name="Uri"></param>
        /// <returns></returns>
        public static async Task RemoveFromPlaylist(string Uri)
        {
            // calls overloaded method
            List<string> tmp = new List<string>();
            tmp.Add(Uri);
            await RemoveFromPlaylist(tmp);
        }


        /// <summary>
        /// Removes list of FullTracks from Playlist
        /// </summary>
        /// <param name="FullTracks"></param>
        /// <returns></returns>
        public static async Task RemoveFromPlaylist(List<FullTrack> FullTracks)
        {
            // makes FullTracks into list of string, calls overloaded method
            List<string> tmp = new List<string>();
            for (int i = 0; i <= FullTracks.Count - 1; i++)
            {
                tmp.Add(FullTracks[i].Uri);
            }
            await RemoveFromPlaylist(tmp);
        }


        /// <summary>
        /// Removes list of Uris from Playlist
        /// </summary>
        /// <param name="Uris"></param>
        /// <returns></returns>
        public static async Task RemoveFromPlaylist(List<string> Uris)
        {
            try
            {
                // define PlaylistRemoveitemsRequest and the Tracks property
                PlaylistRemoveItemsRequest PRIR = new PlaylistRemoveItemsRequest();
                PRIR.Tracks = new List<PlaylistRemoveItemsRequest.Item>();


                foreach (string Uri in Uris)
                {
                    // define PlaylistRemoveItemsRequest Item and add to PlaylistRemoveItemRequest
                    PlaylistRemoveItemsRequest.Item PRIRI = new PlaylistRemoveItemsRequest.Item();
                    PRIRI.Uri = Uri;
                    PRIR.Tracks.Add(PRIRI);
                }

                // call to api to remove
                await MySpotifyClient.Playlists.RemoveItems(Options.SPOTIFY_PLAYLIST_ID, PRIR);
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
        }

        #endregion



        #region GetTrackResponse

        /// <summary>
        /// Gets Information of FullTracks from List of Uris
        /// </summary>
        /// <param name="Uris"></param>
        /// <returns></returns>
        public static async Task<List<FullTrack>> GetTracksReponse(List<string> Uris)
        {
            List<FullTrack> FullTracks = new List<FullTrack>();
            try
            {
                if (!Helper.ListHelper.CanContinueWithList(ref Uris))
                {
                    return FullTracks;
                }

                // Convert Uris to IDs
                List<string> IDs = new List<string>();
                for (int i = 0; i <= Uris.Count - 1; i++)
                {
                    string tmp = Helper.SpotifyHelper.GetTrackIdFromTrackUri(Uris[i]);
                    IDs.Add(tmp);
                }

                // If  ID list is not empty / null etc.
                if (Helper.ListHelper.CanContinueWithList(ref IDs))
                {
                    // request songs from spotify
                    TracksRequest TRequest = new TracksRequest(IDs);
                    TracksResponse TResponse = await MySpotifyClient.Tracks.GetSeveral(TRequest);
                    if (TResponse.Tracks != null)
                    {
                        // return if not null
                        return TResponse.Tracks;
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
            return FullTracks;
        }


        #endregion






        /// <summary>
        /// Removing the oldest songs to make sure the Playlist keeps at its maximum song list
        /// </summary>
        /// <returns></returns>
        public static async Task<List<FullTrack>> KeepSongLimit()
        {
            // remove older songs if limit of playlist songs is reached

            // List of Tracks we removed, and list of Uris of the same thing
            List<FullTrack> RemovedTracks = new List<FullTrack>();
            List<string> Uris = new List<string>();
            try
            {
                // get all tracks currently in the playlist
                List<FullTrack> FullTracks = await GetAllPlaylistFullTracks();

                // if more songs in playlist than we want
                if (FullTracks.Count > Options.SPOTIFY_PLAYLIST_NUMBER_OF_SONGS)
                {
                    // loop through the songs we want to remove
                    for (int i = 0; i <= ((FullTracks.Count - Options.SPOTIFY_PLAYLIST_NUMBER_OF_SONGS) - 1); i++)
                    {
                        // add to both lists
                        Uris.Add(FullTracks[i].Uri);
                        RemovedTracks.Add(FullTracks[i]);
                    }

                    // actually remove from playlist
                    await RemoveFromPlaylist(Uris);
                }
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
            // return Tracks we removed
            return RemovedTracks;
        }





        /// <summary>
        /// Checks if an AlbumID contains only one Song, if so, returns the Uri
        /// </summary>
        /// <param name="AlbumID"></param>
        /// <returns></returns>
        public static async Task<string> GetTrackUriFromAlbum(string AlbumID)
        {
            // If an album only has one Track, return it
            try
            {
                // Get full album
                FullAlbum MyFullAlbum = await MySpotifyClient.Albums.Get(AlbumID);

                // return that track as Uri
                Paging<SimpleTrack> MyAlbumTracks = MyFullAlbum.Tracks;

                if (MyAlbumTracks.Items != null)
                {
                    // if only has one track
                    if (MyAlbumTracks.Items.Count == 1)
                    {
                        // return that uri
                        return MyAlbumTracks.Items[0].Uri;
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
        /// Gets a Track URI from a Spotify Search
        /// </summary>
        /// <param name="SearchString"></param>
        /// <returns></returns>
        public static async Task<string> GetUriFromSearch(string SearchString)
        {
            if (String.IsNullOrWhiteSpace(SearchString))
            {
                return "";
            }
            try
            {
                // Some Magic to make the API work
                SearchRequest.Types mySearchType = SearchRequest.Types.Track;
                SearchRequest MySearchRequest = new SearchRequest(mySearchType, SearchString);
                ISearchClient MySearchClient = MySpotifyClient.Search;
                SearchResponse MySearchResponse = await MySearchClient.Item(MySearchRequest);

                // Convert SearchResults into FullTracks
                List<FullTrack> mySpotifySearchReturns = new List<FullTrack>();

                if (MySearchResponse.Tracks.Items != null)
                {
                    foreach (var tmp in MySearchResponse.Tracks.Items)
                    {
                        if (tmp is FullTrack Track)
                        {
                            mySpotifySearchReturns.Add(tmp);
                        }
                    }
                }


                // Loop through the fulltracks
                string MyClosestLink = "";
                string MyClosestString = "";
                int bestComparison = 9999;
                for (int i = 0; i <= mySpotifySearchReturns.Count - 1; i++)
                {
                    // Get String from Fulltrack we can compare to
                    string SpotifyResultComparisonString = Helper.SpotifyHelper.GetTrackSearchString(mySpotifySearchReturns[i]);

                    // get comparison score
                    int currComparison = Helper.FileHandling.getLevenshteinDistance(SpotifyResultComparisonString, SearchString);

                    //Globals.DebugPrint("SearchString: '{0}', SpotifySearchResult: '{1}', LevenshteinDistance: '{2}'", SearchString, SpotifyResultComparisonString, currComparison);


                    // Do normal logic with getting the best result
                    if (currComparison < bestComparison)
                    {
                        MyClosestLink = mySpotifySearchReturns[i].Uri;
                        MyClosestString = SpotifyResultComparisonString;
                        bestComparison = currComparison;
                    }

                    // if something is good enough return immediately
                    if (currComparison <= Options.MAX_LEVENSHTEIN_DISTANCE)
                    {
                        MyClosestLink = mySpotifySearchReturns[i].Uri;
                        break;
                    }


                    // if we get to the last index, and we havent broken out of the loop
                    if (i == mySpotifySearchReturns.Count - 1)
                    {
                        if (Options.SHOW_ACTIVITY_INTERNAL)
                        {
                            // log what was the closest match even tho we didnt use it
                            string Descripton = "Could not find anything lower or equal than our\nOptions.MAX_LEVENSHTEIN_DISTANCE of " + Options.MAX_LEVENSHTEIN_DISTANCE + "\n";
                            Descripton = Descripton + "\nSearchString: '" + SearchString + "'";
                            Descripton = Descripton + "\nClosestMatch: '" + MyClosestString + "'";
                            Descripton = Descripton + "\nLevenshtein Comparison: '" + bestComparison + "'";
                            Descripton = Descripton + "\nLink of that: '" + MyClosestLink + "'";
                            APIs.MyDiscord.SendMessage(Helper.DiscordHelper.BuildEmbed(null, Descripton, null, Helper.DiscordHelper.EmbedColors.LoggingEmbed));
                        }

                    }
                }

                // make sure we dont return garbage
                if (!String.IsNullOrWhiteSpace(MyClosestLink))
                {
                    return MyClosestLink;
                }

            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
            return "";
        }




        /// <summary>
        /// Check if a Track is in our Playlist already
        /// </summary>
        /// <param name="SongUri"></param>
        /// <returns></returns>
        public static async Task<bool> IsSongInPlaylist(string SongUri)
        {
            if (String.IsNullOrWhiteSpace(SongUri))
            {
                return false;
            }
            try
            {
                List<FullTrack> FullTracks = await GetAllPlaylistFullTracks();
                for (int i = 0; i <= FullTracks.Count - 1; i++)
                {
                    if (FullTracks[i].Uri == SongUri)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }

            return false;
        }


    }
}
