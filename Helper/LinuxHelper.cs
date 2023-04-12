using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CliWrap;

namespace WasIchHoerePlaylist.Helper
{
    internal class LinuxHelper
    {
        /// <summary>
        /// Restarts the Bot itself (linux service)
        /// </summary>
        public static void RestartService()
        {
            var cmd = Cli.Wrap("/bin/systemctl")
                    .WithArguments(new[] { "restart", "grrdiscordspotifyplaylist" })
                    .ExecuteAsync();
        }


        /// <summary>
        /// Shutdowns the Bot itself (linux service)
        /// </summary>
        public static void ShutdownService()
        {
            var cmd = Cli.Wrap("/bin/systemctl")
                    .WithArguments(new[] { "stop", "grrdiscordspotifyplaylist" })
                    .ExecuteAsync();
        }
    }
}
