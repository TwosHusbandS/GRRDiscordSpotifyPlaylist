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

        private static void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                APIs.MyDiscord.SendMessage(Globals.BuildEmbed(null, "Just reset all User limits at midnight", null, Globals.EmbedColors.LoggingEmbed));
                System.Timers.Timer MyOldTimer = (System.Timers.Timer)sender;
                MyOldTimer.Stop();
                MyOldTimer.Dispose();

                Timer_Start();
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
        }

        private static void HitMidnight()
        {
            UserSongs.Reset();
            Backups.AutoCreate();
        }
    }
}
