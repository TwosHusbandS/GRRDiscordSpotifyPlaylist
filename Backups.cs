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
        public static Task List(SocketSlashCommand command = null)
        {
            try
            {
                List<KeyValuePair<string, string>> MyList = new List<KeyValuePair<string, string>>();
                string tmp = "";
                string[] BackupFiles = Helper.FileHandling.GetFilesFromFolder(Globals.BackupPlaylistPath);
                List<string> BackupNames = new List<string>();
                foreach (string BackupFile in BackupFiles)
                {
                    string tmp2 = BackupFile.Substring(Globals.BackupPlaylistPath.Length + 1);
                    BackupNames.Add(tmp2);
                }

                if (BackupNames.Count > 0)
                {
                    foreach (string BackupName in BackupNames)
                    {
                        tmp = tmp + "'" + BackupName + "'" + "\n";
                    }
                    tmp.TrimEnd('\n');
                    MyList.Add(new KeyValuePair<string, string>("Backups:", tmp));
                    Output(Globals.BuildEmbed(command, null, MyList, Globals.EmbedColors.NormalEmbed), command);
                }
                else
                {
                    MyList.Add(new KeyValuePair<string, string>("Error", "No Backups available"));
                    Output(Globals.BuildEmbed(command, null, MyList, Globals.EmbedColors.ErrorEmbed), command);
                }
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }
            return Task.CompletedTask;
        }


        public static async Task Create(SocketSlashCommand command = null, string pBackupName = "")
        {
            try
            {
                string BackupName = "";
                if (GetBackupName(out BackupName, command, pBackupName))
                {
                    Output(Globals.BuildEmbed(command, "Error creating Backup. Name is null or empty.", null, Globals.EmbedColors.ErrorEmbed), command);
                    return;
                }

                string FileName = Helper.FileHandling.PathCombine(Globals.BackupPlaylistPath, BackupName);

                while (Helper.FileHandling.doesFileExist(FileName))
                {
                    FileName = FileName + " (new)";
                }

                List<FullTrack> FullTracks = await APIs.MySpotify.GetAllPlaylistFullTracks();
                List<string> Uris = new List<string>();
                for (int i = 0; i <= FullTracks.Count - 1; i++)
                {
                    Uris.Add(FullTracks[i].Uri);
                }

                Helper.FileHandling.WriteStringToFileOverwrite(FileName, Uris.ToArray());

                Output(Globals.BuildEmbed(command, "Created Backup named: '" + BackupName.ToString() + "'.", null, Globals.EmbedColors.NormalEmbed), command);
            }
            catch (Exception ex)
            {
                Output(Globals.BuildEmbed(command, "Error creating Backup", null, Globals.EmbedColors.ErrorEmbed), command);
                Helper.Logger.Log(ex);
            }
        }


        public static async Task Apply(SocketSlashCommand command = null, string pBackupName = "")
        {
            try
            {
                string BackupName = "";
                if (GetBackupName(out BackupName, command, pBackupName))
                {
                    Output(Globals.BuildEmbed(command, "Error applying Backup. Name is null or empty.", null, Globals.EmbedColors.ErrorEmbed), command);
                    return;
                }

                string FileName = Helper.FileHandling.PathCombine(Globals.BackupPlaylistPath, BackupName);

                if (Helper.FileHandling.doesFileExist(FileName))
                {
                    List<FullTrack> FullTracks = await APIs.MySpotify.GetAllPlaylistFullTracks();
                    await APIs.MySpotify.RemoveFromPlaylist(FullTracks);

                    string[] BackupUris = Helper.FileHandling.ReadFileEachLine(FileName);
                    await APIs.MySpotify.AddToPlaylist(BackupUris.ToList<string>());

                    Output(Globals.BuildEmbed(command, "Applied Backup named: '" + BackupName.ToString() + "'.", null, Globals.EmbedColors.NormalEmbed), command);
                }
                else
                {
                    Output(Globals.BuildEmbed(command, "Backup named: '" + BackupName.ToString() + "' does not exist.", null, Globals.EmbedColors.ErrorEmbed), command);
                }
                Output(Globals.BuildEmbed(command, "Error appying Backup.", null, Globals.EmbedColors.ErrorEmbed), command);
            }
            catch (Exception ex)
            {
                Output(Globals.BuildEmbed(command, "Error appying Backup", null, Globals.EmbedColors.ErrorEmbed), command);
                Helper.Logger.Log(ex);
            }
            return;
        }


        public static Task Delete(SocketSlashCommand command = null, string pBackupName = "")
        {
            try
            {
                string BackupName = "";
                if (GetBackupName(out BackupName, command, pBackupName))
                {
                    Output(Globals.BuildEmbed(command, "Error deleting Backup. Name is null or empty.", null, Globals.EmbedColors.ErrorEmbed), command);
                    return Task.CompletedTask;
                }

                string Path = Helper.FileHandling.PathCombine(Globals.BackupPlaylistPath, BackupName);
                if (Helper.FileHandling.doesFileExist(Path))
                {
                    Helper.FileHandling.deleteFile(Path);
                    Output(Globals.BuildEmbed(command, "Deleted Backup named: '" + BackupName + "'.", null, Globals.EmbedColors.NormalEmbed), command);
                }
                else
                {
                    Output(Globals.BuildEmbed(command, "No Backup named: '" + BackupName + "' found.", null, Globals.EmbedColors.ErrorEmbed), command);
                }
            }
            catch (Exception ex)
            {
                Output(Globals.BuildEmbed(command, "Error deleting Backup", null, Globals.EmbedColors.ErrorEmbed), command);
                Helper.Logger.Log(ex);
            }
            return Task.CompletedTask;
        }


        public static Task AutoCreate()
        {
            try
            {
                string[] BackupFiles = Helper.FileHandling.GetFilesFromFolder(Globals.BackupPlaylistPath);
                string BackupRegexPattern = @"\d{4}_\d{2}_\d{2}_autobackup";
                //foreach (Match MySpotifyMatch in MySpotifyRegex.Matches(Content))
                //{
                //if (MySpotifyMatch.Success)

                List<string> BackupNames = new List<string>();
                foreach (string BackupFile in BackupFiles)
                {
                    string tmp2 = BackupFile.Substring(Globals.BackupPlaylistPath.Length + 1);
                    Match m = Regex.Match(tmp2, BackupRegexPattern);
                    if (m.Success)
                    {
                        BackupNames.Add(tmp2);
                    }
                }

                BackupNames.Sort();
                BackupNames.Reverse();

                // if we have at least 4 backups
                if (BackupNames.Count > 3)
                {
                    // start at index 3, so 4rth backup
                    for (int i = 3; i < BackupNames.Count; i++)
                    {
                        string FileName = Helper.FileHandling.PathCombine(Globals.BackupPlaylistPath, BackupNames[i]);
                        APIs.MyDiscord.SendMessage(Globals.BuildEmbed(null, "Deleted old backup: '" + BackupNames[i] + "' automatically.", null, Globals.EmbedColors.LoggingEmbed));
                        Helper.FileHandling.deleteFile(FileName);
                    }
                }

                string TodaysName = DateTime.Now.ToString("yyyy_MM_dd") + "_autobackup";
                string TodaysFile = Helper.FileHandling.PathCombine(Globals.BackupPlaylistPath, TodaysName);
                if (Helper.FileHandling.doesFileExist(TodaysFile))
                {
                    APIs.MyDiscord.SendMessage(Globals.BuildEmbed(null, "Deleted Todays backup: '" + TodaysName + "', since we will be creating a new one.", null, Globals.EmbedColors.LoggingEmbed));
                    Helper.FileHandling.deleteFile(TodaysFile);
                }
                Create(null, TodaysName);
            }
            catch (Exception ex)
            {
                APIs.MyDiscord.SendMessage(Globals.BuildEmbed(null, "Autobackupcreate failed.", null, Globals.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }

            return Task.CompletedTask;
        }



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
                    tmp = (command.Data.Options.First().Options.First().Value).ToString();
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
