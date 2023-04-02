using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WasIchHoerePlaylist
{
    internal class MidnightTimer
    {


        /// <summary>
        /// Inits the class. Basically making sure it resets at midnight.
        /// </summary>
        public static void Init()
        {
            Timer_Start();
        }


        /// <summary>
        /// Methods that Starts a new Timer.
        /// </summary>
        private static void Timer_Start()
        {
            _ = Task.Run(async () =>
            {
                DateTime now = DateTime.Now;
                DateTime midnight = DateTime.Today.AddDays(1);
                TimeSpan ts = midnight - now;
                System.Timers.Timer MyTimer = new System.Timers.Timer();
                MyTimer.Interval = ts.TotalMilliseconds;
                MyTimer.Elapsed += Timer_Elapsed;
                MyTimer.Start();
            });
        }


        /// <summary>
        /// Method that runs if Timer Elapsed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                // Method we want to execute at midnight
                HitMidnight();

                // Dispose of old Stuff
                System.Timers.Timer MyOldTimer = (System.Timers.Timer)sender;
                MyOldTimer.Stop();
                MyOldTimer.Dispose();

                // Start new Timer
                Timer_Start();
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
        }

        private static Task HitMidnight()
        {
            // Reset User Songs
            UserSongs.Reset();

            // Output
            APIs.MyDiscord.SendMessage(Helper.DiscordHelper.BuildEmbed(null, "Just reset all User limits at midnight.\nWill do autobackup now.", null, Helper.DiscordHelper.EmbedColors.LoggingEmbed));
            
            // Create Auto Backups
            Backups.AutoCreate();

            return Task.CompletedTask;
        }
    }
}
