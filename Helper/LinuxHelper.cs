using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WasIchHoerePlaylist.Helper
{
    internal class LinuxHelper
    {
        /// <summary>
        /// Executes a Command on Linux
        /// </summary>
        /// <param name="cmd"></param>
        public static void ExecuteLinuxCommand(string cmd)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "/bin/bash", Arguments = cmd, };
                Process proc = new Process() { StartInfo = startInfo, };
                proc.Start();
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
        }


        /// <summary>
        /// Restarts the Bot itself (linux service)
        /// </summary>
        public static void RestartService()
        {
            Helper.LinuxHelper.ExecuteLinuxCommand("systemctl restart grrdiscordspotifyplaylist.service");
        }


        /// <summary>
        /// Shutdowns the Bot itself (linux service)
        /// </summary>
        public static void ShutdownService()
        {
            Helper.LinuxHelper.ExecuteLinuxCommand("systemctl stop grrdiscordspotifyplaylist.service");

        }
    }
}
