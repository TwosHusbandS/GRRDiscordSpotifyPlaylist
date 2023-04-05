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



        public static string Bash(string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return result;
        }


        public static string RunCommandWithBash(string command)
        {
            try
            {
                var psi = new ProcessStartInfo();
                psi.FileName = "/bin/bash";
                psi.Arguments = command;
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;

                using var process = Process.Start(psi);

                process.WaitForExit();

                var output = process.StandardOutput.ReadToEnd();

                return output;
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
            return "";
        }


        /// <summary>
        /// Restarts the Bot itself (linux service)
        /// </summary>
        public static void RestartService()
        {
            Helper.LinuxHelper.RunCommandWithBash("systemctl restart grrdiscordspotifyplaylist.service");
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
