using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace WasIchHoerePlaylist.APIs
{

    /// <summary>
    /// Class for our Youtube API
    /// </summary>
    internal class MyYoutube
    {
        // Our YoutubeServer
        static YouTubeService myYoutubeService = null;


        /// <summary>
        /// Method to initialise the YoutubeService
        /// </summary>
        /// <returns></returns>
        public static async Task Init()
        {
            try
            {
                myYoutubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = WasIchHoerePlaylist.Options.YOUTUBE_API_KEY,
                    ApplicationName = "DIRM"
                });
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
        }


        /// <summary>
        /// Gets Title from a Youtube Video
        /// </summary>
        /// <param name="videoId"></param>
        /// <returns></returns>
        public static async Task<string> GetTitleFromVideoId(string videoId)
        {
            string rtrn = "";
            try
            {
                VideosResource.ListRequest listRequest = myYoutubeService.Videos.List("snippet");
                listRequest.Id = videoId;
                VideoListResponse response = await listRequest.ExecuteAsync();
                if (response.Items.Count > 0)
                {
                    rtrn = response.Items[0].Snippet.Title;
                }
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
            return rtrn;
        }
    }
}
