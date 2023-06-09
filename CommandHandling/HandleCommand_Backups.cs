﻿using Discord.WebSocket;
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
        /// <summary>
        /// Handle Backup Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="UserIsAdmin"></param>
        /// <returns></returns>
        static Task HandleCommand_Backups(SocketSlashCommand command, bool UserIsAdmin)
        {
            // return if no permissions
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
        }


        /// <summary>
        /// Handle Backup List Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="UserIsAdmin"></param>
        /// <returns></returns>
        static Task HandleCommand_Backups_List(SocketSlashCommand command, bool UserIsAdmin)
        {
            Backups.List(command);
            return Task.CompletedTask;
        }


        /// <summary>
        /// Handle Backup Create Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="UserIsAdmin"></param>
        /// <returns></returns>
        static Task HandleCommand_Backups_Create(SocketSlashCommand command, bool UserIsAdmin)
        {
            Backups.Create(command);
            return Task.CompletedTask;
        }


        /// <summary>
        /// Handle Backup Delete Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="UserIsAdmin"></param>
        /// <returns></returns>
        static Task HandleCommand_Backups_Delete(SocketSlashCommand command, bool UserIsAdmin)
        {
            Backups.Delete(command);
            return Task.CompletedTask;
        }


        /// <summary>
        /// Handle Backup Apply Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="UserIsAdmin"></param>
        /// <returns></returns>
        static Task HandleCommand_Backups_Apply(SocketSlashCommand command, bool UserIsAdmin)
        {
            Backups.Apply(command);
            return Task.CompletedTask;
        }
    }
}
