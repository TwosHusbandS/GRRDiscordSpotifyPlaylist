using System;
using WasIchHoerePlaylist;
using WasIchHoerePlaylist.APIs;
using WasIchHoerePlaylist.Helper;

//namespace WasIchHoerePlaylist // Note: actual namespace depends on the project name.
//{
//    internal class Program
//    {
//        public static void Main(string[] args)
//        {
//            SetUp();
//        }

//        private static async void SetUp()
//        {

Console.WriteLine("Hello World!");
Console.OutputEncoding = System.Text.Encoding.UTF8;
Logger.Init();
Options.Init();
Globals.Init();
MidnightTimer.Init();
await MyYoutube.Init();
await MySpotify.Init();
MyDiscord.Init();

//        }
//    }
//}


/*

Plans:


- implement:
- - Add Output for adding song when UserLimit is reached and we dont add because of that.

- Check all helps, classes, etc. apart from HandleCommand_Settings
- fill with logging and error catch
- Make everything pretty, Logging, ErrorCatch on commands

loglevel...1 is just errors and exceptions, 2 is warnings, 3 is everything

 */