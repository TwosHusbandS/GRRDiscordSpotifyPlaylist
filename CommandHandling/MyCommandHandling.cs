using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using Swan.Logging;
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
        public static Task SlashCommandHandler(SocketSlashCommand command)
        {
            //await command.RespondAsync($"You executed {command.Data.Name}");
            bool UserIsAdmin = false;
            try
            {
                var user = command.User as SocketGuildUser;
                if (user.GuildPermissions.BanMembers)
                {
                    UserIsAdmin = true;
                }


                switch (command.CommandName)
                {
                    case "help":
                        HandleCommand_Help(command, UserIsAdmin);
                        break;
                    case "status":
                        HandleCommand_Status(command, UserIsAdmin);
                        break;
                    case "playlist":
                        HandleCommand_Playlist(command, UserIsAdmin);
                        break;
                    case "songs":
                        HandleCommand_Songs(command, UserIsAdmin);
                        break;
                    case "backups":
                        HandleCommand_Backups(command, UserIsAdmin);
                        break;
                    case "settings":
                        HandleCommand_Settings(command, UserIsAdmin);
                        break;
                    case "dailyuserlimit":
                        HandleCommand_DailyUserLimit(command, UserIsAdmin);
                        break;
                    case "shutdown":
                        HandleCommand_Shutdown(command, UserIsAdmin);
                        break;
                    case "restart":
                        HandleCommand_Restart(command, UserIsAdmin);
                        break;
                }
            }
            catch
            {
                command.RespondAsync(embed: Globals.BuildEmbed(command, "Error processing slashcommand", null, Globals.EmbedColors.ErrorEmbed));
            }


            return Task.CompletedTask;
        }



        public static Task MissingPermissions(SocketSlashCommand command)
        {
            command.RespondAsync(embed: Globals.BuildEmbed(command, "Error:\nMissing Permissions.", null, Globals.EmbedColors.ErrorEmbed), ephemeral: true);
            return Task.CompletedTask;
        }



        public static async Task BuildCommands(DiscordSocketClient DSC)
        {
            // Let's build a guild command! We're going to need a guild so lets just put that in a variable.
            var guild = DSC.GetGuild(Options.DISCORD_GUILD_ID);

            var CommandHelp = new Discord.SlashCommandBuilder()
            .WithName("help")
            .WithDescription("Shows a help-page.");

            var CommandStatus = new Discord.SlashCommandBuilder()
            .WithName("status")
            .WithDescription("Show the status of the bot!");

            var CommandPlaylist = new Discord.SlashCommandBuilder()
            .WithName("playlist")
            .WithDescription("Link the Spotify Playlist.");

            var CommandSongs = new Discord.SlashCommandBuilder()
            .WithName("songs")
            .WithDescription("Everything to do with the songs of the playlist.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("list")
                .WithDescription("List all Songs of the Playlist")
                .WithType(ApplicationCommandOptionType.SubCommand))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("add")
                .WithDescription("Add a song to the playlist")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("link", ApplicationCommandOptionType.String, "link to the song you want to add", isRequired: true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("remove-at-index")
                .WithDescription("Remove a song from an index from the playlist")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("index", ApplicationCommandOptionType.Integer, "Number of the song you want to remove", isRequired: true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("remove-song")
                .WithDescription("Remove a song from the playlist")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("track", ApplicationCommandOptionType.String, "Spotify Track you want to remove (link, or ID)", isRequired: true));     
            

                var CommandBackups = new Discord.SlashCommandBuilder()
            .WithName("backups")
            .WithDescription("Backups of the songs of the playlist.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("list")
                .WithDescription("List all Backups")
                .WithType(ApplicationCommandOptionType.SubCommand))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("create")
                .WithDescription("Create a backup")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("name", ApplicationCommandOptionType.String, "Name of the backup you want to create", isRequired: true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("delete")
                .WithDescription("Delete a backup")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("name", ApplicationCommandOptionType.String, "Name of the backup you want to delete", isRequired: true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("apply")
                .WithDescription("Use a previous backup and overwrite the playlist")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("name", ApplicationCommandOptionType.String, "Name of the backup you want to use.", isRequired: true));

            var CommandSettings = new Discord.SlashCommandBuilder()
            .WithName("settings")
            .WithDescription("Manage everything settings related.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("show")
                .WithDescription("Show all settings")
                .WithType(ApplicationCommandOptionType.SubCommand))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("re-read")
                .WithDescription("Read all Settings from File again.")
                .WithType(ApplicationCommandOptionType.SubCommand))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("limit")
                .WithDescription("Change the upper limits of this bot.")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("limit_type")
                    .WithDescription("Which limit do you want to edit.")
                    .WithRequired(true)
                    .AddChoice("playlist_amount_of_songs", 1)
                    .AddChoice("user_daily_limit", 2)
                    .WithType(ApplicationCommandOptionType.Integer))
                .AddOption("number", ApplicationCommandOptionType.Integer, "Amount of Songs", isRequired: true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("channel")
                .WithDescription("Pick the public and internal channel")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("channel_type")
                    .WithDescription("Which channel do you want to edit.")
                    .WithRequired(true)
                    .AddChoice("public-channel", 1)
                    .AddChoice("internal-channel", 2)
                    .WithType(ApplicationCommandOptionType.Integer))
                .AddOption("channel_name", ApplicationCommandOptionType.Channel, "Channel you want to use", isRequired: true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("color")
                .WithDescription("Pick the colors of the embeds")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("color-type")
                    .WithDescription("Which color do you want to edit.")
                    .WithRequired(true)
                    .AddChoice("embed", 1)
                    .AddChoice("logging", 2)
                    .AddChoice("error", 3)
                    .WithType(ApplicationCommandOptionType.Integer))
                .AddOption("color", ApplicationCommandOptionType.String, "Color you want the embed to use as Hex with leading #", isRequired: true))
             .AddOption(new SlashCommandOptionBuilder()
                .WithName("log")
                .WithDescription("Pick how much gets logged")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("log-type")
                    .WithDescription("Which logging type.")
                    .WithRequired(true)
                    .AddChoice("discord", 1)
                    .AddChoice("file", 2)
                    .WithType(ApplicationCommandOptionType.Integer))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("log-level")
                    .WithDescription("Which logging level...")
                    .WithRequired(true)
                    .AddChoice("info", 1)
                    .AddChoice("warnings", 2)
                    .AddChoice("errors", 3)
                    .WithType(ApplicationCommandOptionType.Integer)))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("spotify-playlist")
                .WithDescription("Change the Spotify Playlist we modify.")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("keep-songs")
                    .WithDescription("Copy existing Songs to the new Playlist.")
                    .WithRequired(true)
                    .AddChoice("true", 1)
                    .AddChoice("false", 2)
                    .WithType(ApplicationCommandOptionType.Integer))
                .AddOption("playlist", ApplicationCommandOptionType.String, "New Spotify Playlist (ID or Link)", isRequired: true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("maximum-levenshtein-distance")
                .WithDescription("How \"close\" the Spotify Search Response has to be.")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("levenshtein-distance", ApplicationCommandOptionType.Number, "Number of levenshtein-distance", isRequired: true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("discord-guild-id")
                .WithDescription("Change our Discord Guild ID.")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("guild-id", ApplicationCommandOptionType.String, "New Discord Guild ID", isRequired: true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("show-activity-internal")
                .WithDescription("Display added/removed songs in internal Channel.")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("value")
                    .WithDescription("True or False.")
                    .WithRequired(true)
                    .AddChoice("true", 1)
                    .AddChoice("false", 2)
                    .WithType(ApplicationCommandOptionType.Integer)));











            var CommandDailyUserlimit = new Discord.SlashCommandBuilder()
            .WithName("dailyuserlimit")
            .WithDescription("Everything regarding the daily limit of users")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("show")
                .WithDescription("Shows amount of added songs today.")
                .WithType(ApplicationCommandOptionType.SubCommandGroup)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("all")
                    .WithDescription("Shows amount of added songs by all Users today.")
                    .WithType(ApplicationCommandOptionType.SubCommand))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("user")
                    .WithDescription("Shows amount of added songs by a specific Users today.")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("user", ApplicationCommandOptionType.User, "Name of the user", isRequired: true)
                    ))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("reset")
                .WithDescription("Resets amount of added songs today.")
                .WithType(ApplicationCommandOptionType.SubCommandGroup)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("all")
                    .WithDescription("Resets amount of added songs by all Users today.")
                    .WithType(ApplicationCommandOptionType.SubCommand))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("user")
                    .WithDescription("Resets amount of added songs by a specific Users today.")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("user", ApplicationCommandOptionType.User, "Name of the user", isRequired: true)
                    ))
             .AddOption(new SlashCommandOptionBuilder()
                .WithName("set")
                .WithDescription("Set how many songs a user can add per Day.")
                .WithType(ApplicationCommandOptionType.SubCommand));



            var CommandRestart = new Discord.SlashCommandBuilder()
                .WithName("restart")
                .WithDescription("(Tries) restarting the bot.");

            var CommandShutdown = new Discord.SlashCommandBuilder()
                .WithName("shutdown")
                .WithDescription("Shutdowns the bot.");


            /*

            help
            status
            playlist
                        
            songs
            - list
            - add
            - remove

            backups
            - list
            - create
            - apply
            - delete

            settings
            - show
            - limit
            - - playlist-amount-of-songs
            - - user-daily-songs
            - channel
            - - playlist-channel
            - - logging-channel
            - color
            - - color-type
            - log
            - - log-type
            - - log-level
            - spotify_playlist_id
            - - keep-songs
            - discord_guild_id
            - show-activity-internal
            - - true/false value




            */

            try
            {
                var shit = await guild.GetApplicationCommandsAsync();
                var shit2 = await APIs.MyDiscord.MyDiscordClient.GetGlobalApplicationCommandsAsync();

                Console.WriteLine("Removing ALL GUILD commands...");
                foreach (var shi in shit)
                {
                    Console.WriteLine("---- Removing existing GUILD Commands: '" + shi.Name + "'..." );
                    await shi.DeleteAsync();
                    Console.WriteLine("---- DONE Removing existing GUILD Commands: '" + shi.Name + "'...");
                }
                Console.WriteLine("Removing ALL GUILD commands...DONE");

                Console.WriteLine("Removing ALL commands...");

                foreach (var shi2 in shit2)
                {
                    Console.WriteLine("---- Removing existing Commands: '" + shi2.Name + "'...");
                    await shi2.DeleteAsync();
                    Console.WriteLine("---- DONE Removing existing Commands: '" + shi2.Name + "'...");
                }
                Console.WriteLine("Removing ALL commands...DONE");

                //Console.WriteLine("DONE Removing all existing Commands");


                Console.WriteLine("Building ALL commands...");
                await BuildCommand(guild, CommandHelp);
                await BuildCommand(guild, CommandStatus);
                await BuildCommand(guild, CommandPlaylist);
                await BuildCommand(guild, CommandSongs);
                await BuildCommand(guild, CommandBackups);
                await BuildCommand(guild, CommandSettings);
                await BuildCommand(guild, CommandDailyUserlimit);
                await BuildCommand(guild, CommandRestart);
                await BuildCommand(guild, CommandShutdown);
                Console.WriteLine("Building ALL commands...DONE");

            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex, 3);
            }

            return;
        }


        public static Task PrintOptions(SocketSlashCommand command)
        {

            for (int i = 0; i <= command.Data.Options.Count - 1; i++)
            {
                SocketSlashCommandDataOption SSCDO = command.Data.Options.ElementAt(i);
                PrintOptions(SSCDO, i, "");
            }

            return Task.CompletedTask;
        }

        public static Task PrintOptions(SocketSlashCommandDataOption SSCDO, int inde = 0, string Indent = "")
        {
            Console.WriteLine("{0} i={1} SSCDO.Name: '{2}'", Indent, inde, SSCDO.Name);
            Console.WriteLine("{0} i={1} SSCDO.Value: '{2}'", Indent, inde, SSCDO.Value);
            for (int i = 0; i <= SSCDO.Options.Count - 1; i++)
            {
                SocketSlashCommandDataOption SSCDO_ = SSCDO.Options.ElementAt(i);
                PrintOptions(SSCDO_, i, Indent + "    ");
            }
            return Task.CompletedTask;
        }

        public static async Task BuildCommand(SocketGuild SG, SlashCommandBuilder SCB)
        {
            try
            {
                Console.WriteLine("Creating Command: '" + SCB.Name + "'...");
                ApplicationCommandProperties ACP = SCB.Build();
                //await SG.CreateApplicationCommandAsync(ACP);
                await APIs.MyDiscord.MyDiscordClient.CreateGlobalApplicationCommandAsync(ACP);
                Console.WriteLine("Creating Command: '" + SCB.Name + "' DONE");
            }
            catch (Exception e)
            {
                Helper.Logger.Log(e);
            }
        }
    }
}
