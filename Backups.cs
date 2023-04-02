using Discord;
using Discord.WebSocket;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WasIchHoerePlaylist
{
    internal class Backups
    {

        /// <summary>
        /// List all Backups
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Task List(SocketSlashCommand command = null)
        {
            try
            {
                List<KeyValuePair<string, string>> MyList = new List<KeyValuePair<string, string>>();
                string tmp = "";
                string[] BackupFiles = Helper.FileHandling.GetFilesFromFolder(Globals.BackupPlaylistPath);
                List<string> BackupNames = new List<string>();

                // Loop through all BackupFiles
                foreach (string BackupFile in BackupFiles)
                {
                    // add the Name to List
                    string tmp2 = BackupFile.Substring(Globals.BackupPlaylistPath.Length + 1);
                    BackupNames.Add(tmp2);
                }

                // if we have backup
                if (BackupNames.Count > 0)
                {
                    // Loop through BackupNames
                    for (int i = 0; i <= BackupNames.Count - 1; i++)
                    {
                        // add to output
                        tmp = tmp + "'" + BackupNames[i] + "'" + "\n";
                    }

                    // output
                    tmp.TrimEnd('\n');
                    MyList.Add(new KeyValuePair<string, string>("Backups:", tmp));
                    Output(Helper.DiscordHelper.BuildEmbed(command, null, MyList, Helper.DiscordHelper.EmbedColors.NormalEmbed), command);
                }
                else
                {
                    MyList.Add(new KeyValuePair<string, string>("Error", "No Backups available"));
                    Output(Helper.DiscordHelper.BuildEmbed(command, null, MyList, Helper.DiscordHelper.EmbedColors.ErrorEmbed), command);
                }
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
            return Task.CompletedTask;
        }



        /// <summary>
        /// Create Backup
        /// </summary>
        /// <param name="command"></param>
        /// <param name="pBackupName"></param>
        /// <returns></returns>
        public static async Task Create(SocketSlashCommand command = null, string pBackupName = "")
        {
            try
            {
                string BackupName = "";
                if (!GetBackupName(out BackupName, command, pBackupName))
                {
                    Output(Helper.DiscordHelper.BuildEmbed(command, "Error creating Backup. Name is null or empty.", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed), command);
                    return;
                }
                string BackupFilePath = Helper.FileHandling.PathCombine(Globals.BackupPlaylistPath, BackupName);
                // generate FilePath

                // if it exists
                while (Helper.FileHandling.doesFileExist(BackupFilePath))
                {
                    // keep adding " (new)" to it
                    BackupFilePath = BackupFilePath + " (new)";
                }

                // Loop through all Tracks in Playlist, add Uri to List<string>
                List<FullTrack> FullTracks = await APIs.MySpotify.GetAllPlaylistFullTracks();
                List<string> Uris = new List<string>();
                for (int i = 0; i <= FullTracks.Count - 1; i++)
                {
                    Uris.Add(FullTracks[i].Uri);
                }

                // write to file
                Helper.FileHandling.WriteStringToFileOverwrite(BackupFilePath, Uris.ToArray());

                // output
                Output(Helper.DiscordHelper.BuildEmbed(command, "Created Backup named: '" + BackupName.ToString() + "'.", null, Helper.DiscordHelper.EmbedColors.NormalEmbed), command);
            }
            catch (Exception ex)
            {
                Output(Helper.DiscordHelper.BuildEmbed(command, "Error creating Backup", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed), command);
                Helper.Logger.Log(ex);
            }
        }


        /// <summary>
        /// Apply a Backup
        /// </summary>
        /// <param name="command"></param>
        /// <param name="pBackupName"></param>
        /// <returns></returns>
        public static async Task Apply(SocketSlashCommand command = null, string pBackupName = "")
        {
            try
            {
                // generating FilePath etc.
                string BackupName = "";
                if (!GetBackupName(out BackupName, command, pBackupName))
                {
                    Output(Helper.DiscordHelper.BuildEmbed(command, "Error applying Backup. Name is null or empty.", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed), command);
                    return;
                }
                string BackupFilePath = Helper.FileHandling.PathCombine(Globals.BackupPlaylistPath, BackupName);

                // if it exists
                if (Helper.FileHandling.doesFileExist(BackupFilePath))
                {
                    // make playlist empty
                    List<FullTrack> FullTracks = await APIs.MySpotify.GetAllPlaylistFullTracks();
                    await APIs.MySpotify.RemoveFromPlaylist(FullTracks);

                    // fill with Uris from BackupFilePath
                    string[] BackupUris = Helper.FileHandling.ReadFileEachLine(BackupFilePath);
                    await APIs.MySpotify.AddToPlaylist(BackupUris.ToList<string>());

                    // output
                    Output(Helper.DiscordHelper.BuildEmbed(command, "Applied Backup named: '" + BackupName.ToString() + "'.", null, Helper.DiscordHelper.EmbedColors.NormalEmbed), command);
                }
                else
                {
                    Output(Helper.DiscordHelper.BuildEmbed(command, "Backup named: '" + BackupName.ToString() + "' does not exist.", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed), command);
                }
                Output(Helper.DiscordHelper.BuildEmbed(command, "Error appying Backup.", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed), command);
            }
            catch (Exception ex)
            {
                Output(Helper.DiscordHelper.BuildEmbed(command, "Error appying Backup", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed), command);
                Helper.Logger.Log(ex);
            }
            return;
        }


        /// <summary>
        /// Deletes a specific Backup
        /// </summary>
        /// <param name="command"></param>
        /// <param name="pBackupName"></param>
        /// <returns></returns>
        public static Task Delete(SocketSlashCommand command = null, string pBackupName = "")
        {
            try
            {
                // Get BackupName, and build BackupFilePath
                string BackupName = "";
                if (!GetBackupName(out BackupName, command, pBackupName))
                {
                    Output(Helper.DiscordHelper.BuildEmbed(command, "Error deleting Backup. Name is null or empty.", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed), command);
                    return Task.CompletedTask;
                }
                string BackupFilePath = Helper.FileHandling.PathCombine(Globals.BackupPlaylistPath, BackupName);

                // if it exists
                if (Helper.FileHandling.doesFileExist(BackupFilePath))
                {
                    // delete and output
                    Helper.FileHandling.deleteFile(BackupFilePath);
                    Output(Helper.DiscordHelper.BuildEmbed(command, "Deleted Backup named: '" + BackupName + "'.", null, Helper.DiscordHelper.EmbedColors.NormalEmbed), command);
                }
                else
                {
                    Output(Helper.DiscordHelper.BuildEmbed(command, "No Backup named: '" + BackupName + "' found.", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed), command);
                }
            }
            catch (Exception ex)
            {
                Output(Helper.DiscordHelper.BuildEmbed(command, "Error deleting Backup", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed), command);
                Helper.Logger.Log(ex);
            }
            return Task.CompletedTask;
        }



        /// <summary>
        /// Gets called at midnight
        /// </summary>
        /// <returns></returns>
        public static Task AutoCreate()
        {
            try
            {
                // Gets all Files in BackupPath
                string[] BackupFiles = Helper.FileHandling.GetFilesFromFolder(Globals.BackupPlaylistPath);
                string BackupRegexPattern = @"\d{4}_\d{2}_\d{2}_autobackup";
                List<string> AutoBackupNames = new List<string>();
                
                foreach (string BackupFile in BackupFiles)
                {
                    string BackupFileName = BackupFile.Substring(Globals.BackupPlaylistPath.Length + 1);
                    Match m = Regex.Match(BackupFileName, BackupRegexPattern);
                    if (m.Success)
                    {
                        // if it matches regex
                        // add to AutoBackupNames
                        AutoBackupNames.Add(BackupFileName);
                    }
                }

                // Sort and Revers list
                AutoBackupNames.Sort();
                AutoBackupNames.Reverse();


                // if we have at least 4 backups
                if (AutoBackupNames.Count > 3)
                {
                    // start at index 3, so 4rth backup
                    for (int i = 3; i < AutoBackupNames.Count; i++)
                    {
                        // build FileName
                        string FileName = Helper.FileHandling.PathCombine(Globals.BackupPlaylistPath, AutoBackupNames[i]);
                        
                        // delete file
                        Helper.FileHandling.deleteFile(FileName);
                        
                        // Output
                        APIs.MyDiscord.SendMessage(Helper.DiscordHelper.BuildEmbed(null, "Deleted old backup: '" + AutoBackupNames[i] + "' automatically.", null, Helper.DiscordHelper.EmbedColors.LoggingEmbed));
                    }
                }

                // Build file for today
                string TodaysName = DateTime.Now.ToString("yyyy_MM_dd") + "_autobackup";
                string TodaysFile = Helper.FileHandling.PathCombine(Globals.BackupPlaylistPath, TodaysName);
                
                // if it exists already
                if (Helper.FileHandling.doesFileExist(TodaysFile))
                {
                    // delete and inform
                    Helper.FileHandling.deleteFile(TodaysFile);
                    APIs.MyDiscord.SendMessage(Helper.DiscordHelper.BuildEmbed(null, "Deleted Todays backup: '" + TodaysName + "', since we will be creating a new one.", null, Helper.DiscordHelper.EmbedColors.LoggingEmbed));
                }

                // Create Backup for today with our name
                Create(null, TodaysName);
            }
            catch (Exception ex)
            {
                APIs.MyDiscord.SendMessage(Helper.DiscordHelper.BuildEmbed(null, "Autobackupcreate failed.", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }

            return Task.CompletedTask;
        }







        /// <summary>
        /// Output Method. Either outputs to logging Channel or as reponse to command.
        /// </summary>
        /// <param name="pEmbed"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Task Output(Embed pEmbed, SocketSlashCommand command = null)
        {
            try
            {
                if (command == null)
                {
                    APIs.MyDiscord.SendMessage(pEmbed);
                }
                else
                {
                    command.RespondAsync(embed: pEmbed);
                }
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }

            return Task.CompletedTask;
        }



        /// <summary>
        /// Gets Backup Name. Either from SlashCommand or from Parameter
        /// </summary>
        /// <param name="BackupName"></param>
        /// <param name="command"></param>
        /// <param name="CustomBackupName"></param>
        /// <returns></returns>
        public static bool GetBackupName(out string BackupName, SocketSlashCommand command = null, string CustomBackupName = "")
        {
            string tmp = "";

            try
            {
                if (!String.IsNullOrEmpty(CustomBackupName))
                {
                    tmp = CustomBackupName;
                }

                if (command != null)
                {
                    tmp = command.Data.Options.ElementAt(0).Options.ElementAt(0).Value.ToString();
                }
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }


            if (!String.IsNullOrWhiteSpace(tmp))
            {
                BackupName = tmp;
                return true;
            }
            else
            {
                BackupName = "";
                return false;
            }
        }

    }
}
