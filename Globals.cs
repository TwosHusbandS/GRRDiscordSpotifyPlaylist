using Discord;
using Discord.WebSocket;
using SpotifyAPI.Web;
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
        // Stopwatch for Uptime
        public static Stopwatch sw;

        // Fields in embed for added track. We look for these strings at a different place
        // which is why they are saved in a property
        public static readonly string SongAddedTopline = "React with the X to un-add(remove) the Song from the Playlist";
        public static readonly string SongAddedDescription = "Added the following Song to the Playlist:";

        // discord user ID of our own bot (GRR-Spotify)
        public static readonly ulong OurBotID = 1087090664099029042;

        // discord User ID of the FM Bot
        public static readonly ulong FMBotID = 356268235697553409;


        // emote we do reaction stuff with
        public static IEmote MyReactionEmote = (IEmote)(new Emoji("❌"));

        // version of the bot for output in status
        public static Version MyVersion = new Version(1, 0, 3, 0);


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
        /// Link to the Spotify Playlist
        /// </summary>
        public static string SpotifyPlaylist
        {
            get
            {
                return "https://open.spotify.com/playlist/" + Options.SPOTIFY_PLAYLIST_ID;
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
                // Think these were stuff I used before but didnt work on Linux?
                //return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                //return (Directory.GetParent(ProjectInstallationPathBinary).ToString());
            }
        }





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

            if (!Helper.FileHandling.doesPathExist(Globals.BackupPlaylistPath))
            {
                APIs.MyDiscord.SendMessage(Helper.DiscordHelper.BuildEmbed(null, "Error!!\nBackup Path doesnt exist even tho we tried creating it.", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
            }

            // Starting the Stopwatch for Uptime
            sw = new Stopwatch();
            sw.Start();
        }




        public static void DebugPrint()
        {
            DebugPrint(" ");
        }

        public static void DebugPrint(string strng)
        {
            //Console.WriteLine(strng);
            Helper.Logger.Log(strng, 1);
        }

        public static void DebugPrint(StringBuilder sb)
        {
            DebugPrint(sb.ToString());
        }


    }
}
