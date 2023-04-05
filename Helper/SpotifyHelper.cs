using Discord.WebSocket;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WasIchHoerePlaylist.Helper
{
    internal class SpotifyHelper
    {
        /// <summary>
        /// Gets a Spotify URI string just by a song ID
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static string GetTrackUriFromTrackId(string Id)
        {
            return "spotify:track:" + Id;
        }



        /// <summary>
        /// Gets a Spotify ID from a Track Uri
        /// </summary>
        /// <param name="Uri"></param>
        /// <returns></returns>
        public static string GetTrackIdFromTrackUri(string Uri)
        {
            try
            {
                if (Uri.Contains(':'))
                {
                    return Uri.Substring(Uri.LastIndexOf(':') + 1);
                }
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
            return "Failed to convert. GetTrackIdFromTrackUri";
        }


        /// <summary>
        /// Returns all Artists of a FullTrack as one String
        /// </summary>
        /// <param name="ft"></param>
        /// <param name="delimintor"></param>
        /// <returns></returns>
        public static string GetArtistString(FullTrack ft, string delimintor)
        {
            try
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
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
            return "Failed to convert. GetArtistString";
        }



        /// <summary>
        /// Gets String from FullTrack we can put into search
        /// </summary>
        /// <param name="ft"></param>
        /// <returns></returns>
        public static string GetTrackSearchString(FullTrack ft)
        {
            try
            {
                string TrackName = ft.Name;

                // if we have multiple artists
                if (ft.Artists.Count > 1)
                {
                    // if we have tracktitle with feat
                    if ((TrackName.Contains("(feat")) || (TrackName.Contains("(ft")))
                    {
                        TrackName = TrackName.Substring(0, TrackName.LastIndexOf('('));
                        TrackName = TrackName.TrimEnd(' ');
                    }
                }
                return GetArtistString(ft, ", ") + " " + TrackName;
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
            return "Failed to convert. GetTrackSearchString";
        }


        /// <summary>
        /// Gets nice output with Artist, Track and Album for logging
        /// </summary>
        /// <param name="ft"></param>
        /// <returns></returns>
        public static string GetTrackString(FullTrack ft)
        {
            try
            {
                string artist = GetArtistString(ft, ", ");
                string name = ft.Name;
                string AlbumName = ft.Album.Name;
                return artist + " - " + name + " (" + AlbumName + ")";
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
            return "Failed to convert. GetTrackString.";
        }

    }
}
