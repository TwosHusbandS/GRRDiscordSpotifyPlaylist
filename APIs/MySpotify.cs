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


        /// <summary>
        /// Refreshes the authentication using the json from disk.
        /// </summary>
        /// <returns></returns>
        private static async Task RefreshAuthentication()
        {
            try
            {
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



        /// <summary>
        /// Gets all PlayableItems from a Spotify Playlist
        /// </summary>
        /// <returns></returns>
        public static async Task<List<PlaylistTrack<IPlayableItem>>> GetAllPlaylistTracks()
        {
            try
            {
                Paging<PlaylistTrack<IPlayableItem>> MyTracks = await MySpotifyClient.Playlists.GetItems(Options.SPOTIFY_PLAYLIST_ID);
                return (MyTracks.Items.OrderBy(x => x.AddedAt).ToList());
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
            return new List<PlaylistTrack<IPlayableItem>>();

            // https://johnnycrazy.github.io/SpotifyAPI-NET/docs/pagination/
            //IList<PlaylistTrack<IPlayableItem>> asdf = await MySpotifyClient.PaginateAll(MyTracks);
            //return (asdf.OrderBy(x => x.AddedAt).ToList());

        }



        /// <summary>
        /// Gets all FullTracks from a Spotify Playlist
        /// </summary>
        /// <returns></returns>
        public static async Task<List<FullTrack>> GetAllPlaylistFullTracks()
        {
            List<FullTrack> FullTracks = new List<FullTrack>();

            try
            {
                List<PlaylistTrack<IPlayableItem>> Tracks = await GetAllPlaylistTracks();
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


        /// <summary>
        /// Removing the oldest songs to make sure the Playlist keeps at its maximum song list
        /// </summary>
        /// <returns></returns>
        public static async Task<List<FullTrack>> KeepSongLimit()
        {
            List<FullTrack> RemovedTracks = new List<FullTrack>();
            try
            {
                List<FullTrack> FullTracks = await GetAllPlaylistFullTracks();

                if (FullTracks.Count > Options.SPOTIFY_PLAYLIST_NUMBER_OF_SONGS)
                {
                    List<string> Uris = new List<string>();
                    for (int i = 0; i <= ((FullTracks.Count - Options.SPOTIFY_PLAYLIST_NUMBER_OF_SONGS) - 1); i++)
                    {
                        Uris.Add(FullTracks[i].Uri);
                        RemovedTracks.Add(FullTracks[i]);
                    }
                    await RemoveFromPlaylist(Uris);
                }
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
            return RemovedTracks;
        }


        /// <summary>
        /// Removes list of FullTracks from Playlist
        /// </summary>
        /// <param name="FullTracks"></param>
        /// <returns></returns>
        public static async Task RemoveFromPlaylist(List<FullTrack> FullTracks)
        {
            List<string> tmp = new List<string>();
            for (int i = 0; i <= FullTracks.Count - 1; i++)
            {
                tmp.Add(FullTracks[i].Uri);
            }
            await RemoveFromPlaylist(tmp);
        }


        /// <summary>
        /// Removes a single URI from Playlist
        /// </summary>
        /// <param name="Uri"></param>
        /// <returns></returns>
        public static async Task RemoveFromPlaylist(string Uri)
        {
            List<string> tmp = new List<string>();
            tmp.Add(Uri);
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
                PlaylistRemoveItemsRequest PRIR = new PlaylistRemoveItemsRequest();
                PRIR.Tracks = new List<PlaylistRemoveItemsRequest.Item>();

                foreach (string Uri in Uris)
                {
                    PlaylistRemoveItemsRequest.Item PRIRI = new PlaylistRemoveItemsRequest.Item();
                    PRIRI.Uri = Uri;
                    PRIR.Tracks.Add(PRIRI);
                }

                await MySpotifyClient.Playlists.RemoveItems(Options.SPOTIFY_PLAYLIST_ID, PRIR);
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
        }



        /// <summary>
        /// Checks if an AlbumID contains only one Song, if so, returns the Uri
        /// </summary>
        /// <param name="AlbumID"></param>
        /// <returns></returns>
        public static async Task<string> GetTrackUriFromAlbum(string AlbumID)
        {
            try
            {
                FullAlbum MyFullAlbum = await MySpotifyClient.Albums.Get(AlbumID);
                if (MyFullAlbum.TotalTracks == 1)
                {
                    Paging<SimpleTrack> MyAlbumTracks = MyFullAlbum.Tracks;
                    return MyAlbumTracks.Items[0].Uri;
                }
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }

            return "";
        }


        public static async Task<List<FullTrack>> GetTracksReponse(List<string> Uris)
        {
            List<FullTrack> FullTracks = new List<FullTrack>();
            try
            {
                List<string> IDs = new List<string>();
                for (int i = 0; i <= Uris.Count - 1; i++)
                {
                    string tmp = MySpotify.GetIdFromTrackUri(Uris[i]);
                    IDs.Add(tmp);
                }
                if (Globals.CanContinueWithList(ref IDs))
                {
                    TracksRequest TRequest = new TracksRequest(IDs);
                    TracksResponse TResponse = await MySpotifyClient.Tracks.GetSeveral(TRequest);
                    if (TResponse.Tracks != null)
                    {
                        return TResponse.Tracks;
                    }
                }
            }
            catch
            {
            }
            return FullTracks;
        }


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
            List<string> UriList = new List<string>();
            UriList.Add(Uri);
            await AddToPlaylist(UriList);
        }


        /// <summary>
        /// Gets a Track URI from a Spotify Search
        /// </summary>
        /// <param name="SearchString"></param>
        /// <returns></returns>
        public static async Task<string> GetUriFromSearch(string SearchString)
        {
            if (String.IsNullOrEmpty(SearchString))
            {
                return "";
            }
            try
            {
                SearchRequest.Types mySearchType = SearchRequest.Types.Track;
                SearchRequest MySearchRequest = new SearchRequest(mySearchType, SearchString);
                ISearchClient MySearchClient = MySpotifyClient.Search;
                SearchResponse MySearchResponse = await MySearchClient.Item(MySearchRequest);

                List<string> mySpotifySearchReturnsTitles = new List<string>();
                List<string> mySpotifySearchReturnsArtists = new List<string>();
                List<string> mySpotifySearchReturnsUri = new List<string>();
                foreach (var tmp in MySearchResponse.Tracks.Items)
                {
                    if (tmp is FullTrack Track)
                    {
                        mySpotifySearchReturnsTitles.Add(tmp.Name);
                        mySpotifySearchReturnsArtists.Add(GetArtistString(tmp, " "));
                        mySpotifySearchReturnsUri.Add(tmp.Uri);
                    }
                }

                string MyClosestLink = "";
                //int bestComparison = 9999;
                for (int i = 0; i <= mySpotifySearchReturnsTitles.Count - 1; i++)
                {
                    string ComparisonString = mySpotifySearchReturnsArtists[i] + " " + mySpotifySearchReturnsTitles[i];
                    int currComparison = Helper.FileHandling.getLevenshteinDistance(ComparisonString, SearchString);
                    //Console.WriteLine("String: '{0}', SearchResult: '{1}', LevenshteinDistance: '{2}'", SearchString, ComparisonString, currComparison);

                    if (currComparison <= Options.MAX_LEVENSHTEIN_DISTANCE)
                    {
                        MyClosestLink = mySpotifySearchReturnsUri[i];
                        break;
                    }
                    //if (currComparison < bestComparison)
                    //{
                    //    MyClosestLink = mySpotifySearchReturnsUri[i];
                    //    bestComparison = currComparison;
                    //}
                }

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





        /*
        public static async Task<bool> IsSongInPlaylist(string SongUri)
        {
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
        */



        /// <summary>
        /// Gets a Spotify URI string just by a song ID
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static string GetTrackUriFromId(string Id)
        {
            return "spotify:track:" + Id;
        }

        public static string GetIdFromTrackUri(string Uri)
        {
            string rtrn = "";
            try
            {
                rtrn = Uri.Substring(Uri.LastIndexOf(':') + 1);
            }
            catch { }
            return rtrn;
        }


        /// <summary>
        /// Returns all Artists of a FullTrack as one String
        /// </summary>
        /// <param name="ft"></param>
        /// <param name="delimintor"></param>
        /// <returns></returns>
        public static string GetArtistString(FullTrack ft, string delimintor)
        {
            string AllArtists = "";
            for (int i = 0; i <= ft.Artists.Count - 1; i++)
            {
                AllArtists += ft.Artists[i].Name;
                if (i < ft.Artists.Count - 1)
                {
                    AllArtists += delimintor;
                }
            }
            return AllArtists;
        }

        public static string GetTrackString(FullTrack ft)
        {
            return GetArtistString(ft, ", ") + " - " + ft.Name + " (" + ft.Album.Name + ")";
        }

    }
}
