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
            Backups.List(command);
            return Task.CompletedTask;
        }

        static Task HandleCommand_Backups_Create(SocketSlashCommand command, bool UserIsAdmin)
        {
            Backups.Create(command);
            return Task.CompletedTask;
        }


        static Task HandleCommand_Backups_Delete(SocketSlashCommand command, bool UserIsAdmin)
        {
            Backups.Delete(command);
            return Task.CompletedTask;
        }

        static Task HandleCommand_Backups_Apply(SocketSlashCommand command, bool UserIsAdmin)
        {
            Backups.Apply(command);
            return Task.CompletedTask;
        }
    }
}
