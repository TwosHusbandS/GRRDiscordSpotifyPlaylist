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
        /// <summary>
        /// Hnalde DailyUserLimit Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="UserIsAdmin"></param>
        /// <returns></returns>
        static Task HandleCommand_DailyUserLimit(SocketSlashCommand command, bool UserIsAdmin)
        {
            try
            {
                // return if missing permission
                if (!UserIsAdmin)
                {
                    MissingPermissions(command);
                    return Task.CompletedTask;
                }


                List<KeyValuePair<string, object>> parameter = new List<KeyValuePair<string, object>>();
                List<SocketSlashCommandDataOption> Parameters = command.Data.Options.First().Options.ToList<SocketSlashCommandDataOption>();
                foreach (var Parameter in Parameters)
                {
                    parameter.Add(new KeyValuePair<string, object>(Parameter.Name, Parameter.Value));
                }

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
                MyCommandHandling.RespondAsync(command, Helper.DiscordHelper.BuildEmbed(command, "Error processing DailyUserLimit command", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
                Helper.Logger.Log(ex);
            }
            return Task.CompletedTask;
        }



        /// <summary>
        /// Handle DailyUserLimit Show Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="Parameter"></param>
        /// <returns></returns>
        public static Task HandleDailyUserLimit_Show(SocketSlashCommand command, List<KeyValuePair<string, object>> Parameter)
        {
            try
            {
                // show all user limits
                if (Parameter[0].Key == "all")
                {
                    string output = "";

                    if (UserSongs.ListOfUserSongs.Count > 0)
                    {
                        output = "{User}: {Amount_Of_Added_Songs_Today}\n";
                        foreach (UserSongs US in UserSongs.ListOfUserSongs)
                        {
                            SocketGuildUser SGU = APIs.MyDiscord.MyDiscordClient.GetGuild(Options.DISCORD_GUILD_ID).GetUser(US.UserID);

                            output = output + Helper.DiscordHelper.GetUserTextFromSGU(SGU) + ": " + US.SongCount;
                            output += "\n\nAll users can add: '" + Options.USER_DAILY_LIMIT + "' per Day.\nAll other Users have not added a Song today.";
                        }
                    }
                    else
                    {
                        output = "No User have added a Song today.\nThey can all add '" + Options.USER_DAILY_LIMIT + "' Songs per day.";
                    }
                    output += "\n\nIt is currently: " + DateTime.Now.ToString("HH:mm") + " o'clock (24hrs).";

                    MyCommandHandling.RespondAsync(command, Helper.DiscordHelper.BuildEmbed(command, output, null, Helper.DiscordHelper.EmbedColors.NormalEmbed));
                }
                // only show for one user
                else if (Parameter[0].Key == "user")
                {
                    SocketGuildUser SGU = (SocketGuildUser)command.Data.Options.First().Options.First().Options.First().Value;
                    int Songs = UserSongs.GetUserAdds(SGU.Id);
                    string output = "";
                    output += "The user: '" + Helper.DiscordHelper.GetUserTextFromSGU(SGU) + "'\nhas added '" + Songs + "' of their allowed: '" + Options.USER_DAILY_LIMIT + "' Songs today.";
                    output += "\n\nIt is currently: " + DateTime.Now.ToString("HH:mm") + " o'clock (24hrs).";
                    MyCommandHandling.RespondAsync(command, Helper.DiscordHelper.BuildEmbed(command, output, null, Helper.DiscordHelper.EmbedColors.NormalEmbed));
                }
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
                MyCommandHandling.RespondAsync(command, Helper.DiscordHelper.BuildEmbed(command, "Error while showing DailyUserLimit", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// Handle DailyUserLimit Reset Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="Parameter"></param>
        /// <returns></returns>
        public static Task HandleDailyUserLimit_Reset(SocketSlashCommand command, List<KeyValuePair<string, object>> Parameter)
        {
            try
            {
                // reset all users
                if (Parameter[0].Key == "all")
                {
                    UserSongs.Reset();
                    MyCommandHandling.RespondAsync(command, Helper.DiscordHelper.BuildEmbed(command, "Just reset the amount of Songs for all Users", null, Helper.DiscordHelper.EmbedColors.NormalEmbed));

                }
                // reset one user
                else if (Parameter[0].Key == "user")
                {
                    SocketGuildUser SGU = (SocketGuildUser)command.Data.Options.First().Options.First().Options.First().Value;
                    UserSongs.Reset(SGU.Id);
                    MyCommandHandling.RespondAsync(command, Helper.DiscordHelper.BuildEmbed(command, "Just reset the amount of Songs that were added by:\n" + Helper.DiscordHelper.GetUserTextFromSGU(SGU), null, Helper.DiscordHelper.EmbedColors.NormalEmbed));
                }
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
                MyCommandHandling.RespondAsync(command, Helper.DiscordHelper.BuildEmbed(command, "Error while resetting DailyUserLimit", null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// Handle DailyUserLimit Set Command, telling User what command to use.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="Parameter"></param>
        /// <returns></returns>
        public static Task HandleDailyUserLimit_Set(SocketSlashCommand command, List<KeyValuePair<string, object>> Parameter)
        {
            string Command = "/settings limit limit_type:user_daily_limit number:";
            MyCommandHandling.RespondAsync(command, Helper.DiscordHelper.BuildEmbed(command, "To change how many songs a User can add per day, please use:\n" + Command, null, Helper.DiscordHelper.EmbedColors.ErrorEmbed));
            return Task.CompletedTask;
        }
    }
}