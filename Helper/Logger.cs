using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WasIchHoerePlaylist.Helper
{
    internal class Logger
    {
        // We should probably use a logging libary / framework now that I think about it...whatevs
        // Actually implementing this probably took less time than googling "Logging class c#", and we have more control over it

        /// <summary>
        /// Init Function which gets called once at the start.
        /// </summary>
        public static void Init()
        {

            // Since the createFile Method will override an existing file
            if (!FileHandling.doesFileExist(Globals.Logfile))
            {
                FileHandling.createFile(Globals.Logfile);
            }


            string MyCreationDate = FileHandling.GetCreationDate(Process.GetCurrentProcess().MainModule.FileName).ToString("yyyy-MM-ddTHH:mm:ss");

            Logger.Log("-");
            Logger.Log("-");
            Logger.Log("-");
            Logger.Log(" === Logger thingy started === ");
            Logger.Log("    I was created (non UTC) at: '" + MyCreationDate + "'");
            Logger.Log("    Time Now: '" + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "'");
            Logger.Log("    Time Now UTC: '" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss") + "'");
        }


        /// <summary>
        /// Main Method of Logging.cs which is called to log stuff to console (and file by default)
        /// </summary>
        /// <param name="pLogMessage"></param>
        public static Task Log(string pLogMessage, int pLogLevel = 1, bool DontLogToFile = false, bool DontLogToDiscord = false)
        {
            string MyLogMessage = "[" + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "] - " + pLogMessage;

            Globals.DebugPrint(MyLogMessage);

            bool LogToFile = false;
            bool LogToDiscord = false;

            // loglevel...1 is info, 2 is warning, 3 is error

            // parameter loglevel 2 also warnung
            // settings discord_log_level 3 also error
            // settings file_log_level 1 also info

            if (!DontLogToFile)
            {
                if (pLogLevel >= Options.LOG_LEVEL_FILE)
                {
                    LogToFile = true;
                }    
            }

            if (!DontLogToDiscord)
            {
                if (pLogLevel >= Options.LOG_LEVEL_DISCORD)
                {
                    LogToDiscord = true;
                }
            }


            if (LogToDiscord)
            {
                APIs.MyDiscord.SendMessage(MyLogMessage, Helper.DiscordHelper.EmbedColors.NoEmbed);
            }
            if (LogToFile)
            {
                FileHandling.AddToLog(Globals.Logfile, MyLogMessage);
            }


            return Task.CompletedTask;
        }


        /// <summary>
        /// Logging Method of an exception.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="LogToFile"></param>
        /// <param name="LogToDiscord"></param>
        public static void Log(Exception ex, int pLogLevel = 3, bool DontLogToFile = false, bool DontLogToDiscord = false)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Globals.DebugPrint();
            Globals.DebugPrint("---- ERROR:");
            Globals.DebugPrint(ex.ToString());
            Globals.DebugPrint();
            Console.ResetColor();

            Log("----ERROR:\n" + ex.ToString(), pLogLevel, DontLogToFile, DontLogToDiscord);
        }

    } // End of Class
} // End of NameSpace



