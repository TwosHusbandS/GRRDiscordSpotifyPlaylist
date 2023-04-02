using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WasIchHoerePlaylist.Helper
{
    internal class NetworkHelper
    {
        /// <summary>
        /// Gets final redirect web adress from url
        /// </summary>
        /// <param name="MyUrl"></param>
        /// <returns></returns>
        public static async Task<string> GetFinalRedirectUrlFromUrl(string MyUrl)
        {

            // not much to comment, i stole this somewhere.
            // had to add statuscodes to it tho...suuuucks.

            string newurl = MyUrl;
            bool redirecting = true;

            while (redirecting)
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(newurl);
                    request.AllowAutoRedirect = false;
                    request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.1.3) Gecko/20090824 Firefox/3.5.3 (.NET CLR 4.0.20506)";
                    HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());
                    if ((int)response.StatusCode == 301 || (int)response.StatusCode == 302 || (int)response.StatusCode == 307 || (int)response.StatusCode == 308)
                    {
                        string uriString = response.Headers["Location"];
                        //Console.WriteLine("Redirecting " + newurl + " to " + uriString + " because " + response.StatusCode);
                        newurl = uriString;
                        // and keep going
                    }
                    else
                    {
                        //Console.WriteLine("Not redirecting " + url + " because " + response.StatusCode);
                        redirecting = false;
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("Not redirecting " + url + " because SHIT CRASHED");
                    //Exceptions.ExceptionRecord.ReportWarning(ex); // change this to your own
                    redirecting = false;
                }
            }
            return newurl;
        }
    }
}
