using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WasIchHoerePlaylist
{
    internal class Globals
    {
        public static Stopwatch sw;

        /// <summary>
        /// Initialises the Globals class
        /// </summary>
        public static void Init()
        {
            // Create Backup Path if it doesnt exist
            try
            {
                if (!Helper.FileHandling.doesPathExist(Globals.BackupPlaylistPath))
                {
                    Helper.FileHandling.createPath(Globals.BackupPlaylistPath);
                }
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }

            // Starting the Stopwatch for Uptime
            sw = new Stopwatch();
            sw.Start();
        }


        public static string GetUserText(SocketGuildUser SGU)
        {
            return SGU.Nickname + " (" + SGU.Username + ")'";
        }

        /// <summary>
        /// Gets a formatted String from a Timespan
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static string ToFormattedString(TimeSpan ts)
        {
            const string separator = ", ";
            string rtrn = "";

            try
            {
                if (ts.TotalMilliseconds < 1) { return "No time"; }

                rtrn = string.Join(separator, new string[]
                {
                ts.Days > 0 ? ts.Days + (ts.Days > 1 ? " days" : " day") : null,
                ts.Hours > 0 ? ts.Hours + (ts.Hours > 1 ? " hours" : " hour") : null,
                ts.Minutes > 0 ? ts.Minutes + (ts.Minutes > 1 ? " minutes" : " minute") : null,
                ts.Seconds > 0 ? ts.Seconds + (ts.Seconds > 1 ? " seconds" : " second") : null,
                ts.Milliseconds > 0 ? ts.Milliseconds + (ts.Milliseconds > 1 ? " milliseconds" : " millisecond") : null,
                }.Where(t => t != null));
            }
            catch (Exception ex)
            {
                rtrn = "Failed creating Uptime string.";
            }

            return rtrn;
        }


        /// <summary>
        /// Full Path to our Logfile
        /// </summary>
        public static string Logfile
        {
            get
            {
                return Path.Combine(ProjectInstallationPath.TrimEnd(Path.DirectorySeparatorChar), @"logfile.log");
                //ProjectInstallationPath.TrimEnd('/') + @"/logfile.log";
            }
        }


        /// <summary>
        /// Full Path to our ConfigFile
        /// </summary>
        public static string Configfile
        {
            get
            {
                return Path.Combine(ProjectInstallationPath.TrimEnd(Path.DirectorySeparatorChar), @"config.ini");
                //ProjectInstallationPath.TrimEnd('/') + @"/config.ini";
            }
        }


        /// <summary>
        /// Full Path to our Backup ConfigFile
        /// </summary>
        public static string ConfigfileBak
        {
            get
            {
                return Configfile + ".bak";
                //ProjectInstallationPath.TrimEnd('/') + @"/config.ini";
            }
        }


        /// <summary>
        /// Full Path to our SpotifyTokenFile
        /// </summary>
        public static string SpotifyTokenFile
        {
            get
            {
                return Path.Combine(ProjectInstallationPath.TrimEnd(Path.DirectorySeparatorChar), @"spotifyapi.token");
            }
        }


        /// <summary>
        /// The folder where we save Playlists Backups.
        /// </summary>
        public static string BackupPlaylistPath
        {
            get
            {
                return Helper.FileHandling.PathCombine(ProjectInstallationPath, "backup_playlists");
            }
        }


        /// <summary>
        /// Gets the folder where our binary is in.
        /// </summary>
        public static string ProjectInstallationPath
        {
            get
            {
                return System.AppContext.BaseDirectory;
                //return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                //return (Directory.GetParent(ProjectInstallationPathBinary).ToString());
            }
        }


        /// <summary>
        /// Enum of our EmbedColors
        /// </summary>
        public enum EmbedColors
        {
            NormalEmbed,
            LoggingEmbed,
            ErrorEmbed,
            NoEmbed
        }


        /// <summary>
        /// Building an Embed from Parameters
        /// </summary>
        /// <param name="command"></param>
        /// <param name="pDescription"></param>
        /// <param name="pFields"></param>
        /// <param name="pEmbedColor"></param>
        /// <returns></returns>
        public static Embed BuildEmbed(SocketSlashCommand command, string pDescription, List<KeyValuePair<string, string>> pFields, EmbedColors pEmbedColor)
        {
            Color _color;


            // pEmbedColor could be no embed here, but tbh we dont give a rats ass, put it as Info i guess?
            switch (pEmbedColor)
            {
                case EmbedColors.ErrorEmbed:
                    _color = new Color(Options.ERROR_COLOR.R, Options.ERROR_COLOR.G, Options.ERROR_COLOR.B);
                    break;
                case EmbedColors.LoggingEmbed:
                    _color = new Color(Options.LOG_COLOR.R, Options.LOG_COLOR.G, Options.LOG_COLOR.B);
                    break;
                default:
                    _color = new Color(Options.EMBED_COLOR.R, Options.EMBED_COLOR.G, Options.EMBED_COLOR.B);
                    break;
            }

            var embed = new EmbedBuilder();

            // Or with methods
            embed.WithColor(_color);

            if (command != null)
            {
                embed.WithAuthor(command.User);
            }

            embed.WithTitle("#WasIchHöre - Spotify Bot")
                .WithUrl(Logic.SpotifyPlaylist); // url is on title

            if (!string.IsNullOrEmpty(pDescription))
            {
                embed.WithDescription(pDescription);
            }

            if (!(pFields == null))
            {
                foreach (var field in pFields)
                {
                    embed.AddField(field.Key, field.Value);
                }
            }

            embed.WithFooter(footer => footer.Text = "GermanRapReddit Discord Spotify Bot") // no markdown here
                .WithCurrentTimestamp();

            Embed myEmbed = embed.Build();

            return myEmbed;
        }


        public static string SongAddedTopline = "React with the X to un-add(remove) the Song from the Playlist";
        public static string SongAddedDescription = "Added the following Song to the Playlist:";

        public static ulong OurBotID = 1087090664099029042;

        public static IEmote MyReactionEmote = (IEmote)(new Emoji("❌"));

        public static List<string> RemoveNullEmptyWhitespaceDuplicateStringList(List<string> input)
        {
            return input.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
        }

        public static bool CanContinueWithList(ref List<string> MyList)
        {
            MyList = RemoveNullEmptyWhitespaceDuplicateStringList(MyList);
            if (MyList.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }    
        }


        public static async Task<string> UrlLengthen(string url)
        {
            string newurl = url;

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
                        Console.WriteLine("Redirecting " + newurl + " to " + uriString + " because " + response.StatusCode);
                        newurl = uriString;
                        // and keep going
                    }
                    else
                    {
                        Console.WriteLine("Not redirecting " + url + " because " + response.StatusCode);
                        redirecting = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Not redirecting " + url + " because SHIT CRASHED");
                    //Exceptions.ExceptionRecord.ReportWarning(ex); // change this to your own
                    redirecting = false;
                }
            }
            return newurl;
        }
    }
}
