﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public static async Task<string> GetSearchStringFromYoutubeID(string videoId)
        {
            string rtrn = "";
            try
            {
                // create ListRequest
                VideosResource.ListRequest listRequest = myYoutubeService.Videos.List("snippet");
                
                // fill Request with our ID
                listRequest.Id = videoId;

                // ask API, wait for response
                VideoListResponse response = await listRequest.ExecuteAsync();

                // if we received something
                if (response.Items.Count > 0)
                {
                    // get description from video
                    string dscrpt = response.Items[0].Snippet.Description;
                    
                    // split description into array
                    string[] dscrptArr = dscrpt.Split('\n');

                    // if description has a few lines
                    if (dscrptArr.Length >= 4)
                    {
                        // this code block gets information if the track is autogenerated
                        // so we dont rely on title + channel

                        // Check if first line starts with
                        if (dscrptArr[0].ToLower().StartsWith("provided to youtube by"))
                        {
                            // Check if last line starts with
                            if (dscrptArr[dscrptArr.Length - 1].ToLower().StartsWith("auto-generated"))
                            {
                                // split third line by that special char youtube uses
                                string[] tmp = dscrptArr[2].Split(" · ");

                                // if we have at least two things (artist and trackname)
                                if (tmp.Length >= 2)
                                {
                                    // loop from the second to the end
                                    for (int i = 1; i <= tmp.Length - 1; i++)
                                    {
                                        // add to our rtrn string
                                        rtrn += tmp[i];

                                        // if we are NOT at the end of the loop
                                        if (i != tmp.Length - 1)
                                        {
                                            // add komma
                                            rtrn += ", ";
                                        }
                                    }

                                    // all artists done, lets at track title
                                    rtrn += " ";
                                    rtrn += tmp[0];
                                }
                            }
                        }
                    }


                    // if rtrn is still empty (auto generated description parse didnt work)
                    if (String.IsNullOrEmpty(rtrn))
                    {
                        // set rtrn to title of video
                        rtrn = response.Items[0].Snippet.Title;

                        // remove "(prod. by thS)" and all variants 
                        string RegexProdWithBrackets = @"(\(|\[)prod(.|uced)? (by )?(.)*(\)|\])";
                        Regex RegexProdWithBracketsRegex = new Regex(RegexProdWithBrackets);
                        Match RegexProdWithBracketsMatch = RegexProdWithBracketsRegex.Match(rtrn);
                        if (RegexProdWithBracketsMatch.Success)
                        {
                            // some substring kind of magic?
                            rtrn = rtrn.Remove(RegexProdWithBracketsMatch.Index, RegexProdWithBracketsMatch.Groups[0].Value.ToString().Length);
                        }    

                        // do a lot of string replace stuff
                        rtrn = rtrn.ToLower().Replace("official video", "");
                        rtrn = rtrn.ToLower().Replace("official visualizer", "");
                        rtrn = rtrn.ToLower().Replace("official visualiser", "");
                        rtrn = rtrn.ToLower().Replace("visualiser", "");
                        rtrn = rtrn.ToLower().Replace("visualizer", "");
                        rtrn = rtrn.ToLower().Replace("bass boosted", "");
                        rtrn = rtrn.ToLower().Replace("bass boost", "");
                        rtrn = rtrn.ToLower().Replace("nightcore", "");
                        rtrn = rtrn.ToLower().Replace("ft.", "");
                        rtrn = rtrn.ToLower().Replace("feat.", "feat");
                        rtrn = rtrn.ToLower().Replace("feat", "");
                        rtrn = rtrn.ToLower().Replace("produced by ", "");
                        rtrn = rtrn.ToLower().Replace("prod.", "prod");
                        rtrn = rtrn.ToLower().Replace("prod by ", "");
                        rtrn = rtrn.ToLower().Replace("prod ", "");
                        rtrn = rtrn.ToLower().Replace("produced ", "");
                        rtrn = rtrn.ToLower().Replace("()", "");
                        rtrn = rtrn.ToLower().Replace("[]", "");
                        rtrn = rtrn.ToLower().Replace(" - ", "");

                        // while contains double space
                        while (rtrn.Contains("  "))
                        {
                            // replace double space with solo space
                            // this (while loop) gets rid of tripple spaces etc.
                            rtrn = rtrn.Replace("  ", " ");
                        }
                    }
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
