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
        static Task HandleCommand_DailyUserLimit(SocketSlashCommand command, bool UserIsAdmin)
        {
            try
            {
                if (!UserIsAdmin)
                {
                    MissingPermissions(command);
                    return Task.CompletedTask;
                }


                List<KeyValuePair<string, object>> parameter = new List<KeyValuePair<string, object>>();
                try
                {
                    List<SocketSlashCommandDataOption> Parameters = command.Data.Options.First().Options.ToList<SocketSlashCommandDataOption>();
                    foreach (var Parameter in Parameters)
                    {
                        parameter.Add(new KeyValuePair<string, object>(Parameter.Name, Parameter.Value));
                    }
                }
                catch { }

                switch (command.Data.Options.First().Name)
                {
                    case "show":
                        HandleDailyUserLimit_Show(command, parameter);
                        break;
                    case "reset":
                        HandleDailyUserLimit_Reset(command, parameter);
                        break;
                    case "set":
                        HandleDailyUserLimit_Set(command, parameter);
                        break;
                }
            }
            catch (Exception ex)
            {
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Error processing DailyUserLimit command", null, Globals.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
            return Task.CompletedTask;
        }


        public static Task HandleDailyUserLimit_Show(SocketSlashCommand command, List<KeyValuePair<string, object>> Parameter)
        {
            try
            {
               
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Error while showing DailyUserLimit", null, Globals.EmbedColors.ErrorEmbed));
            }
            return Task.CompletedTask;
        }

        public static Task HandleDailyUserLimit_Reset(SocketSlashCommand command, List<KeyValuePair<string, object>> Parameter)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Error while resetting DailyUserLimit", null, Globals.EmbedColors.ErrorEmbed));
            }
            return Task.CompletedTask;
        }

        public static Task HandleDailyUserLimit_Set(SocketSlashCommand command, List<KeyValuePair<string, object>> Parameter)
        {
            string Command = "/settings limit limit_type:user_daily_limit number:";
            command.RespondAsync(embed: Globals.BuildEmbed(command, "To change how many songs a User can add per day, please use:\n" + Command, null, Globals.EmbedColors.ErrorEmbed));
            return Task.CompletedTask;
        }
    }
}