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
            }
            catch { }

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
                case "dailylimitreset":
                    HandleCommand_ResetDailyLimit(command, UserIsAdmin);
                    break;
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
                .WithName("remove")
                .WithDescription("Remove a song from the playlist")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("index", ApplicationCommandOptionType.Integer, "Number of the song you want to remove", isRequired: true));

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


            var CommandReset = new Discord.SlashCommandBuilder()
            .WithName("dailylimitreset")
            .WithDescription("Resets the daily limit of users")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("all")
                .WithDescription("Resets the daily limit for all users")
                .WithType(ApplicationCommandOptionType.SubCommand))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("user")
                .WithDescription("Resets the daily limit for a specific user")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("user", ApplicationCommandOptionType.User, "Name of the backup you want to create", isRequired: true));


            //case "discord_playlist_id":
            // "discord_guild_id":
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
                //var shit = await guild.GetApplicationCommandsAsync();
                //Console.WriteLine();
                //Console.WriteLine("Removing all existing Commands...");
                //foreach (var shi in shit)
                //{
                //    //if (shi.Name == "settings")
                //    //{
                //    Console.WriteLine("---- Removing existing Commands: '" + shi.Name + "'..." );
                //    await shi.DeleteAsync();
                //    Console.WriteLine("---- DONE Removing existing Commands: '" + shi.Name + "'...");
                //
                //
                //    //}
                //}
                //Console.WriteLine("DONE Removing all existing Commands");



                //BuildCommand(guild, CommandHelp.Build());
                //BuildCommand(guild, CommandStatus.Build());
                //BuildCommand(guild, CommandPlaylist.Build());
                //BuildCommand(guild, CommandSongs.Build());
                //BuildCommand(guild, CommandBackups.Build());
                //BuildCommand(guild, CommandSettings.Build());
                //BuildCommand(guild, CommandReset);
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex, 3);
            }

            return;
        }

        public static async Task BuildCommand(SocketGuild SG, SlashCommandBuilder SCB)
        {
            try
            {
                Console.WriteLine("Creating Command: '" + SCB.Name + "'...");
                ApplicationCommandProperties ACP = SCB.Build();
                await SG.CreateApplicationCommandAsync(ACP);
                Console.WriteLine("Creating Command: '" + SCB.Name + "' DONE");
            }
            catch (Exception e)
            {
                Helper.Logger.Log(e);
            }
        }
    }
}
