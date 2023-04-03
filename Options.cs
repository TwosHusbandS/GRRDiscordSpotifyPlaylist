using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;

namespace WasIchHoerePlaylist
{
    internal class Options
    {
        private static readonly ulong DEFAULT_DISCORD_GUILD_ID = 553326164668710931;
        private static readonly ulong DEFAULT_DISCORD_INTERNAL_CHANNEL = 1087092340885295176;
        private static readonly ulong DEFAULT_DISCORD_PUBLIC_CHANNEL = 1072640198422311004;
        private static readonly System.Drawing.Color DEFAULT_EMBED_COLOR = System.Drawing.ColorTranslator.FromHtml("#522783");
        private static readonly System.Drawing.Color DEFAULT_LOG_COLOR = System.Drawing.ColorTranslator.FromHtml("#ffba26");
        private static readonly System.Drawing.Color DEFAULT_ERROR_COLOR = System.Drawing.ColorTranslator.FromHtml("#ff0000");
        private static readonly int DEFAULT_SPOTIFY_PLAYLIST_NUMBER_OF_SONGS = 50;
        private static readonly int DEFAULT_USER_DAILY_LIMIT = 3;
        private static readonly int DEFAULT_LOG_LEVEL_DISCORD = 3;
        private static readonly int DEFAULT_LOG_LEVEL_FILE = 3;
        private static readonly int DEFAULT_MAX_LEVENSHTEIN_DISTANCE = 15;
        private static readonly bool DEFAULT_SHOW_ACTIVITY_INTERNAL = true;

        public static string SPOTIFY_CLIENT_ID = "";
        public static string SPOTIFY_CLIENT_SECRET = "";
        public static string SPOTIFY_PLAYLIST_ID = "";
        public static int SPOTIFY_PLAYLIST_NUMBER_OF_SONGS = DEFAULT_SPOTIFY_PLAYLIST_NUMBER_OF_SONGS;

        public static string YOUTUBE_API_KEY = "";

        public static string DISCORD_OAUTH_CLIENT_ID = "";
        public static string DISCORD_OAUTH_CLIENT_SECRET = "";
        public static string DISCORD_BOT_TOKEN = "";
        public static ulong DISCORD_GUILD_ID = DEFAULT_DISCORD_GUILD_ID;
        public static ulong DISCORD_INTERNAL_CHANNEL = DEFAULT_DISCORD_INTERNAL_CHANNEL;
        public static ulong DISCORD_PUBLIC_CHANNEL = DEFAULT_DISCORD_PUBLIC_CHANNEL;

        public static System.Drawing.Color EMBED_COLOR = DEFAULT_EMBED_COLOR;
        public static System.Drawing.Color LOG_COLOR = DEFAULT_LOG_COLOR;
        public static System.Drawing.Color ERROR_COLOR = DEFAULT_ERROR_COLOR;

        public static int USER_DAILY_LIMIT = DEFAULT_USER_DAILY_LIMIT;

        public static int LOG_LEVEL_DISCORD = DEFAULT_LOG_LEVEL_DISCORD;
        public static int LOG_LEVEL_FILE = DEFAULT_LOG_LEVEL_FILE;
        public static int MAX_LEVENSHTEIN_DISTANCE = DEFAULT_MAX_LEVENSHTEIN_DISTANCE;
        public static bool SHOW_ACTIVITY_INTERNAL = DEFAULT_SHOW_ACTIVITY_INTERNAL;

        // grr purple as hex and rgb int
        // #522783 - 82, 39, 131



        /// <summary>
        /// Inits the Setting Class
        /// </summary>
        public static void Init()
        {
            ReadFromFile();
        }

        /// <summary>
        /// Reads all Settings from File
        /// </summary>
        public static void ReadFromFile()
        {
            try
            {
            
                string configFileContent = Helper.FileHandling.ReadContentOfFile(Globals.Configfile);
            
                Options.SPOTIFY_CLIENT_ID = Helper.FileHandling.GetXMLTagContent(configFileContent, "SPOTIFY_CLIENT_ID");
                Options.SPOTIFY_CLIENT_SECRET = Helper.FileHandling.GetXMLTagContent(configFileContent, "SPOTIFY_CLIENT_SECRET");
                Options.SPOTIFY_PLAYLIST_ID = Helper.FileHandling.GetXMLTagContent(configFileContent, "SPOTIFY_PLAYLIST_ID");
                if (!(Int32.TryParse(Helper.FileHandling.GetXMLTagContent(configFileContent, "SPOTIFY_PLAYLIST_NUMBER_OF_SONGS"), out Options.SPOTIFY_PLAYLIST_NUMBER_OF_SONGS)))
                {
                    Options.SPOTIFY_PLAYLIST_NUMBER_OF_SONGS = Options.DEFAULT_SPOTIFY_PLAYLIST_NUMBER_OF_SONGS;
                }
            
                Options.YOUTUBE_API_KEY = Helper.FileHandling.GetXMLTagContent(configFileContent, "YOUTUBE_API_KEY");
            
                Options.DISCORD_OAUTH_CLIENT_ID = Helper.FileHandling.GetXMLTagContent(configFileContent, "DISCORD_OAUTH_CLIENT_ID");
                Options.DISCORD_OAUTH_CLIENT_SECRET = Helper.FileHandling.GetXMLTagContent(configFileContent, "DISCORD_OAUTH_CLIENT_SECRET");
                Options.DISCORD_BOT_TOKEN = Helper.FileHandling.GetXMLTagContent(configFileContent, "DISCORD_BOT_TOKEN");
                if (!(ulong.TryParse(Helper.FileHandling.GetXMLTagContent(configFileContent, "DISCORD_GUILD_ID"), out Options.DISCORD_GUILD_ID)))
                {
                    Options.DISCORD_GUILD_ID = DEFAULT_DISCORD_GUILD_ID;
                }
                if (!(ulong.TryParse(Helper.FileHandling.GetXMLTagContent(configFileContent, "DISCORD_INTERNAL_CHANNEL"), out Options.DISCORD_INTERNAL_CHANNEL)))
                {
                    Options.DISCORD_INTERNAL_CHANNEL = DEFAULT_DISCORD_INTERNAL_CHANNEL;
                }
                if (!(ulong.TryParse(Helper.FileHandling.GetXMLTagContent(configFileContent, "DISCORD_PUBLIC_CHANNEL"), out Options.DISCORD_PUBLIC_CHANNEL)))
                {
                    Options.DISCORD_PUBLIC_CHANNEL = DEFAULT_DISCORD_PUBLIC_CHANNEL;
                }
            
                if (!(TryParseColor(Helper.FileHandling.GetXMLTagContent(configFileContent, "EMBED_COLOR"), out Options.EMBED_COLOR)))
                {
                    Options.EMBED_COLOR = DEFAULT_EMBED_COLOR;
                }
                if (!(TryParseColor(Helper.FileHandling.GetXMLTagContent(configFileContent, "LOG_COLOR"), out Options.LOG_COLOR)))
                {
                    Options.LOG_COLOR = DEFAULT_LOG_COLOR;
                }
                if (!(TryParseColor(Helper.FileHandling.GetXMLTagContent(configFileContent, "ERROR_COLOR"), out Options.ERROR_COLOR)))
                {
                    Options.ERROR_COLOR = DEFAULT_ERROR_COLOR;
                }
            
                if (!(Int32.TryParse(Helper.FileHandling.GetXMLTagContent(configFileContent, "USER_DAILY_LIMIT"), out Options.USER_DAILY_LIMIT)))
                {
                    Options.USER_DAILY_LIMIT = Options.DEFAULT_USER_DAILY_LIMIT;
                }
                if (!(Int32.TryParse(Helper.FileHandling.GetXMLTagContent(configFileContent, "LOG_LEVEL_DISCORD"), out Options.LOG_LEVEL_DISCORD)))
                {
                    Options.LOG_LEVEL_DISCORD = Options.DEFAULT_LOG_LEVEL_DISCORD;
                }
                if (!(Int32.TryParse(Helper.FileHandling.GetXMLTagContent(configFileContent, "LOG_LEVEL_FILE"), out Options.LOG_LEVEL_FILE)))
                {
                    Options.LOG_LEVEL_FILE = Options.DEFAULT_LOG_LEVEL_FILE;
                }
                if (!(Int32.TryParse(Helper.FileHandling.GetXMLTagContent(configFileContent, "MAX_LEVENSHTEIN_DISTANCE"), out Options.MAX_LEVENSHTEIN_DISTANCE)))
                {
                    Options.MAX_LEVENSHTEIN_DISTANCE = Options.DEFAULT_MAX_LEVENSHTEIN_DISTANCE;
                }
                if (Helper.FileHandling.GetXMLTagContent(configFileContent, "SHOW_ACTIVITY_INTERNAL").ToLower() == "false")
                {
                    SHOW_ACTIVITY_INTERNAL = false;
                }
                else
                {
                    SHOW_ACTIVITY_INTERNAL = true;
                }
            
                string logstring = "---- read all settings:";
                logstring += " Options.SPOTIFY_CLIENT_ID='" + Options.SPOTIFY_CLIENT_ID + "'";
                logstring += " Options.SPOTIFY_CLIENT_SECRET='" + Options.SPOTIFY_CLIENT_SECRET + "'";
                logstring += " Options.SPOTIFY_PLAYLIST_ID='" + Options.SPOTIFY_PLAYLIST_ID + "'";
                logstring += " Options.SPOTIFY_PLAYLIST_NUMBER_OF_SONGS='" + Options.SPOTIFY_PLAYLIST_NUMBER_OF_SONGS + "'";
                logstring += " Options.YOUTUBE_API_KEY='" + Options.YOUTUBE_API_KEY + "'";
                logstring += " Options.DISCORD_OAUTH_CLIENT_ID='" + Options.DISCORD_OAUTH_CLIENT_ID + "'";
                logstring += " Options.DISCORD_OAUTH_CLIENT_SECRET='" + Options.DISCORD_OAUTH_CLIENT_SECRET + "'";
                logstring += " Options.DISCORD_BOT_TOKEN='" + Options.DISCORD_BOT_TOKEN + "'";
                logstring += " Options.DISCORD_GUILD_ID='" + Options.DISCORD_GUILD_ID + "'";
                logstring += " Options.DISCORD_INTERNAL_CHANNEL='" + Options.DISCORD_INTERNAL_CHANNEL + "'";
                logstring += " Options.DISCORD_PUBLIC_CHANNEL='" + Options.DISCORD_PUBLIC_CHANNEL + "'";
                logstring += " Options.EMBED_COLOR='" + ColorToHex(Options.EMBED_COLOR) + "'";
                logstring += " Options.LOG_COLOR='" + ColorToHex(Options.LOG_COLOR) + "'";
                logstring += " Options.ERROR_COLOR='" + ColorToHex(Options.ERROR_COLOR) + "'";
                logstring += " Options.USER_DAILY_LIMIT='" + Options.USER_DAILY_LIMIT + "'";
                logstring += " Options.LOG_LEVEL_DISCORD='" + Options.LOG_LEVEL_DISCORD + "'";
                logstring += " Options.LOG_LEVEL_FILE='" + Options.LOG_LEVEL_FILE + "'";
                logstring += " Options.MAX_LEVENSHTEIN_DISTANCE='" + Options.MAX_LEVENSHTEIN_DISTANCE + "'";
                logstring += " Options.SHOW_ACTIVITY_INTERNAL='" + Options.SHOW_ACTIVITY_INTERNAL + "'";
            
                Helper.Logger.Log(logstring, 1, false, true);
            }
            catch (Exception e)
            {
                Helper.Logger.Log(e);
            }
        }


        /// <summary>
        /// Trying to parse a String (Hex) as System.Drawing.Color
        /// </summary>
        /// <param name="Hex"></param>
        /// <param name="_Color"></param>
        /// <returns></returns>
        public static bool TryParseColor(string Hex, out System.Drawing.Color _Color)
        {
            try
            {
                if (Hex.Length == 6)
                {
                    Hex = "#" + Hex;
                }

                if (Hex.Length == 7)
                {
                    _Color = System.Drawing.ColorTranslator.FromHtml(Hex);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
            _Color = default(System.Drawing.Color);
            return false;
        }


        /// <summary>
        /// Writes all Settings to File
        /// </summary>
        public static void WriteToFile()
        {
            List<string> settings = new List<string>();
            settings.Add("<SPOTIFY_CLIENT_ID>" + Options.SPOTIFY_CLIENT_ID + "</SPOTIFY_CLIENT_ID>");
            settings.Add("<SPOTIFY_CLIENT_SECRET>" + Options.SPOTIFY_CLIENT_SECRET + "</SPOTIFY_CLIENT_SECRET>");
            settings.Add("<SPOTIFY_PLAYLIST_ID>" + Options.SPOTIFY_PLAYLIST_ID + "</SPOTIFY_PLAYLIST_ID>");
            settings.Add("<SPOTIFY_PLAYLIST_NUMBER_OF_SONGS>" + Options.SPOTIFY_PLAYLIST_NUMBER_OF_SONGS + "</SPOTIFY_PLAYLIST_NUMBER_OF_SONGS>");
            settings.Add("<YOUTUBE_API_KEY>" + Options.YOUTUBE_API_KEY + "</YOUTUBE_API_KEY>");
            settings.Add("<DISCORD_OAUTH_CLIENT_ID>" + Options.DISCORD_OAUTH_CLIENT_ID + "</DISCORD_OAUTH_CLIENT_ID>");
            settings.Add("<DISCORD_OAUTH_CLIENT_SECRET>" + Options.DISCORD_OAUTH_CLIENT_SECRET + "</DISCORD_OAUTH_CLIENT_SECRET>");
            settings.Add("<DISCORD_BOT_TOKEN>" + Options.DISCORD_BOT_TOKEN + "</DISCORD_BOT_TOKEN>");
            settings.Add("<DISCORD_GUILD_ID>" + Options.DISCORD_GUILD_ID + "</DISCORD_GUILD_ID>");
            settings.Add("<DISCORD_INTERNAL_CHANNEL>" + Options.DISCORD_INTERNAL_CHANNEL + "</DISCORD_INTERNAL_CHANNEL>");
            settings.Add("<DISCORD_PUBLIC_CHANNEL>" + Options.DISCORD_PUBLIC_CHANNEL + "</DISCORD_PUBLIC_CHANNEL>");
            settings.Add("<EMBED_COLOR>" + ColorToHex(Options.EMBED_COLOR) + "</EMBED_COLOR>");
            settings.Add("<LOG_COLOR>" + ColorToHex(Options.LOG_COLOR) + "</LOG_COLOR>");
            settings.Add("<ERROR_COLOR>" + ColorToHex(Options.ERROR_COLOR) + "</ERROR_COLOR>");
            settings.Add("<USER_DAILY_LIMIT>" + Options.USER_DAILY_LIMIT + "</USER_DAILY_LIMIT>");
            settings.Add("<LOG_LEVEL_DISCORD>" + Options.LOG_LEVEL_DISCORD + "</LOG_LEVEL_DISCORD>");
            settings.Add("<LOG_LEVEL_FILE>" + Options.LOG_LEVEL_FILE + "</LOG_LEVEL_FILE>");
            settings.Add("<MAX_LEVENSHTEIN_DISTANCE>" + Options.MAX_LEVENSHTEIN_DISTANCE + "</MAX_LEVENSHTEIN_DISTANCE>");
            settings.Add("<SHOW_ACTIVITY_INTERNAL>" + Options.SHOW_ACTIVITY_INTERNAL + "</SHOW_ACTIVITY_INTERNAL>");

            try
            {
                Helper.FileHandling.deleteFile(Globals.ConfigfileBak);
                Helper.FileHandling.moveFile(Globals.Configfile, Globals.ConfigfileBak);
                Helper.FileHandling.WriteStringToFileOverwrite(Globals.Configfile, settings.ToArray());
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
        }


        /// <summary>
        /// Generates a string to show all (non sensitive) Settings.
        /// </summary>
        /// <returns></returns>
        public static string SettingsListCommandOutput()
        {
            string logstring = "";
            logstring += "SPOTIFY_PLAYLIST_ID='" + Options.SPOTIFY_PLAYLIST_ID + "'";
            logstring += "\nDISCORD_GUILD_ID='" + Options.DISCORD_GUILD_ID + "'";
            logstring += "\nLIMIT_PLAYLIST_AMOUNT_OF_SONGS='" + Options.SPOTIFY_PLAYLIST_NUMBER_OF_SONGS + "'";
            logstring += "\nLIMIT_USER_DAILY_SONGS='" + Options.USER_DAILY_LIMIT + "'";
            logstring += "\nCHANNEL_PLAYLIST_CHANNEL_='" + Options.DISCORD_PUBLIC_CHANNEL + "'";
            logstring += "\nCHANNEL_LOGGING_CHANNEL='" + Options.DISCORD_INTERNAL_CHANNEL + "'";
            logstring += "\nCOLOR_EMBED='" + Options.ColorToHex(Options.EMBED_COLOR) + "'";
            logstring += "\nCOLOR_LOG='" + Options.ColorToHex(Options.LOG_COLOR) + "'";
            logstring += "\nCOLOR_ERROR='" + Options.ColorToHex(Options.ERROR_COLOR) + "'";
            logstring += "\nLOG_LEVEL_DISCORD='" + Options.LOG_LEVEL_DISCORD + "'";
            logstring += "\nLOG_LEVEL_FILE='" + Options.LOG_LEVEL_FILE + "'";
            logstring += "\nMAX_LEVENSHTEIN_DISTANCE='" + Options.MAX_LEVENSHTEIN_DISTANCE + "'";
            logstring += "\nSHOW_ACTIVITY_INTERNAL='" + Options.SHOW_ACTIVITY_INTERNAL + "'";

            return logstring;
        }


        /// <summary>
        /// Converting a Color to a Hex String
        /// </summary>
        /// <param name="_color"></param>
        /// <returns></returns>
        public static string ColorToHex(Color _color)
        {
            try
            {
                return "#" + _color.R.ToString("X2") + _color.G.ToString("X2") + _color.B.ToString("X2");
            }
            catch
            {
                return "Color conversion failed";
            }
        }
    }
}
