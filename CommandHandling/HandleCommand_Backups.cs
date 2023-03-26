using Discord.WebSocket;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WasIchHoerePlaylist.CommandHandling
{
    internal partial class MyCommandHandling
    {
        static Task HandleCommand_Backups(SocketSlashCommand command, bool UserIsAdmin)
        {
            if (!UserIsAdmin)
            {
                MissingPermissions(command);
                return Task.CompletedTask;
            }

            var fieldName = command.Data.Options.First().Name;
            switch (fieldName)
            {
                case "list":
                    {
                        HandleCommand_Backups_List(command, UserIsAdmin);
                    }
                    break;
                case "create":
                    {
                        HandleCommand_Backups_Create(command, UserIsAdmin);
                    }
                    break;
                case "apply":
                    {
                        HandleCommand_Backups_Apply(command, UserIsAdmin);
                    }
                    break;
                case "delete":
                    {
                        HandleCommand_Backups_Delete(command, UserIsAdmin);
                    }
                    break;
            }


            return Task.CompletedTask;
            /*
            var CommandBackups = new Discord.SlashCommandBuilder()
            .WithName("backups")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("list")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("create")
                .AddOption("name", ApplicationCommandOptionType.String, "Name of the backup you want to create", isRequired: true)
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("delete")
                .AddOption("name", ApplicationCommandOptionType.String, "Name of the backup you want to delete", isRequired: true)
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("apply")
                .AddOption("name", ApplicationCommandOptionType.String, "Name of the backup you want to use.", isRequired: true);
            */
        }


        static Task HandleCommand_Backups_List(SocketSlashCommand command, bool UserIsAdmin)
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
                        tmp += "'";
                        tmp += BackupName;
                        tmp += "'";
                        tmp += "\n";
                    }
                    tmp.TrimEnd('\n');
                    MyList.Add(new KeyValuePair<string, string>("Backups:", tmp));
                    command.RespondAsync(embed: Globals.BuildEmbed(command, null, MyList, Globals.EmbedColors.NormalEmbed));
                }
                else
                {
                    MyList.Add(new KeyValuePair<string, string>("Error", "No Backups available"));
                    command.RespondAsync(embed: Globals.BuildEmbed(command, null, MyList, Globals.EmbedColors.ErrorEmbed));
                }
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
            }

            return Task.CompletedTask;
        }

        static async Task HandleCommand_Backups_Create(SocketSlashCommand command, bool UserIsAdmin)
        {
            try
            {
                string BackupName = (command.Data.Options.First().Options.First().Value).ToString();

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

                command.RespondAsync(embed: Globals.BuildEmbed(command, "Created Backup named: '" + BackupName.ToString() + "'.", null, Globals.EmbedColors.NormalEmbed));
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Error creating Backup", null, Globals.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }

        }


        static Task HandleCommand_Backups_Delete(SocketSlashCommand command, bool UserIsAdmin)
        {
            try
            {
                var Name = command.Data.Options.First().Options.First().Value;
                string Path = Helper.FileHandling.PathCombine(Globals.BackupPlaylistPath, Name.ToString());
                if (Helper.FileHandling.doesFileExist(Path))
                {
                    Helper.FileHandling.deleteFile(Path);
                    command.RespondAsync(embed: Globals.BuildEmbed(command, "Deleted Backup named: '" + Name.ToString() + "'.", null, Globals.EmbedColors.NormalEmbed));
                }
                else
                {
                    command.RespondAsync(embed: Globals.BuildEmbed(command, "No Backup named: '" + Name.ToString() + "' found.", null, Globals.EmbedColors.ErrorEmbed));
                }
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Error deleting Backup", null, Globals.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
            return Task.CompletedTask;
        }

        static async Task HandleCommand_Backups_Apply(SocketSlashCommand command, bool UserIsAdmin)
        {
            try
            {
                string BackupName = (command.Data.Options.First().Options.First().Value).ToString();
                string FileName = Helper.FileHandling.PathCombine(Globals.BackupPlaylistPath, BackupName);

                if (Helper.FileHandling.doesFileExist(FileName))
                {
                    List<FullTrack> FullTracks = await APIs.MySpotify.GetAllPlaylistFullTracks();
                    await APIs.MySpotify.RemoveFromPlaylist(FullTracks);

                    string[] BackupUris = Helper.FileHandling.ReadFileEachLine(FileName);
                    await APIs.MySpotify.AddToPlaylist(BackupUris.ToList<string>());

                    command.RespondAsync(embed: Globals.BuildEmbed(command, "Applied Backup named: '" + BackupName.ToString() + "'.", null, Globals.EmbedColors.NormalEmbed));
                }
                else
                {
                    command.RespondAsync(embed: Globals.BuildEmbed(command, "Backup named: '" + BackupName.ToString() + "' does not exist.", null, Globals.EmbedColors.ErrorEmbed));
                }
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Error appying Backup.", null, Globals.EmbedColors.ErrorEmbed));
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Error appying Backup", null, Globals.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
            return;
        }
    }
}
