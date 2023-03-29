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
            //Console.WriteLine("A");
            try
            {
                //Console.WriteLine("B");
                VideosResource.ListRequest listRequest = myYoutubeService.Videos.List("snippet");
                listRequest.Id = videoId;
                VideoListResponse response = await listRequest.ExecuteAsync();
                //Console.WriteLine("C");
                if (response.Items.Count > 0)
                {
                    //Console.WriteLine("D");
                    string dscrpt = response.Items[0].Snippet.Description;
                    string[] arr = dscrpt.Split('\n');
                    //Console.WriteLine("E");
                    if (arr.Length >= 4)
                    {
                        //Console.WriteLine("F");
                        if (arr[0].ToLower().StartsWith("provided to youtube by"))
                        {
                            //Console.WriteLine("G");

                            if (arr[arr.Length - 1].ToLower().StartsWith("auto-generated"))
                            {
                                string[] tmp = arr[2].Split(" · ");
                                if (tmp.Length >= 2)
                                {
                                    for (int i = 1; i <= tmp.Length - 1; i++)
                                    {
                                        rtrn += tmp[i];

                                        if (i != tmp.Length - 1)
                                        {
                                            rtrn += ", ";
                                        }
                                    }
                                    rtrn += " ";
                                    rtrn += tmp[0];
                                }
                            }
                        }
                    }
                    if (String.IsNullOrEmpty(rtrn))
                    {
                        rtrn = response.Items[0].Snippet.Title;
                        rtrn.ToLower().Replace("official video", "");
                    }
                    //Console.WriteLine("=====");
                    //Console.WriteLine(rtrn);
                    //Console.WriteLine("=====");
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("ZZZZ");
                Helper.Logger.Log(ex);
            }
            return rtrn;
        }
    }
}
