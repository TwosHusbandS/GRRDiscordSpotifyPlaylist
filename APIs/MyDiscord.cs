using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using Swan.Logging;

namespace WasIchHoerePlaylist.APIs
{
    /// <summary>
    /// Class for our Discord Bot and interacting with the Discord API
    /// </summary>
    internal class MyDiscord
    {
        // our Client
        public static DiscordSocketClient MyDiscordClient;

        // the prev. message we keep track of
        // due to fmbot messages counting to the limit counting for author of the message before
        static SocketMessage prevMessageParam = null;

        // Discord.Net heavily utilizes TAP for async, so we create
        // an asynchronous context from the beginning.
        public static void Init()
            => new MyDiscord()
                .MainAsync()
                .GetAwaiter()
                .GetResult();

        /// <summary>
        /// Constructor
        /// </summary>
        public MyDiscord()
        {
            // Config used by DiscordSocketClient
            // Define intents for the client
            var config = new DiscordSocketConfig
            {
                //GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
                GatewayIntents = GatewayIntents.GuildBans | GatewayIntents.GuildEmojis | GatewayIntents.GuildIntegrations | GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageTyping | GatewayIntents.Guilds | GatewayIntents.GuildVoiceStates | GatewayIntents.MessageContent
            };

            // It is recommended to Dispose of a client when you are finished
            // using it, at the end of your app's lifetime.
            MyDiscordClient = new DiscordSocketClient(config);

            // Subscribing to client events, so that we may receive them whenever they're invoked.
            MyDiscordClient.Log += LogAsync;
            MyDiscordClient.Ready += ReadyAsync;
            MyDiscordClient.MessageReceived += MessageReceivedAsync;
            MyDiscordClient.MessageUpdated += MyDiscordClient_MessageUpdated;
            MyDiscordClient.SlashCommandExecuted += SlashCommandExecuted;
            MyDiscordClient.ReactionAdded += ReactionAdded;
        }




        /// <summary>
        /// Runs after the Constructor of MyDiscord
        /// </summary>
        /// <returns></returns>
        public async Task MainAsync()
        {
            // Tokens should be considered secret data, and never hard-coded.
            await MyDiscordClient.LoginAsync(TokenType.Bot, Options.DISCORD_BOT_TOKEN);

            // Start the client
            await MyDiscordClient.StartAsync();

            // Block the program until it is closed.
            await Task.Delay(Timeout.Infinite);
        }


        /// <summary>
        /// DiscordBot Logging Message. Logs to console. 
        /// If LogSeverity.Error logs to File and Discord, if Warning logs to File.
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        private Task LogAsync(LogMessage log)
        {
            StringBuilder sb = new StringBuilder($"{DateTime.Now,-19} [{log.Severity,8}] {log.Source}: {log.Message} {log.Exception}");
            int LogLevel = 0;
            switch (log.Severity)
            {

                case LogSeverity.Critical:
                case LogSeverity.Error:
                    LogLevel = 3;
                    break;
                case LogSeverity.Warning:
                    LogLevel = 2;
                    break;
                case LogSeverity.Info:
                    LogLevel = 1;
                    break;
            }


            if (LogLevel != 0)
            {
                Helper.Logger.Log(sb.ToString(), LogLevel);
            }

            return Task.CompletedTask;
        }


        /// <summary>
        /// Sends a message. If EmbedColor is null, sends as non-embed
        /// </summary>
        /// <param name="MyLogMessage"></param>
        /// <param name="pEmbedColor"></param>
        /// <returns></returns>
        public static async Task<RestUserMessage> SendMessage(string MyLogMessage, Globals.EmbedColors pEmbedColor)
        {
            RestUserMessage RUM;
            if (pEmbedColor == Globals.EmbedColors.NoEmbed)
            {
                RUM = await APIs.MyDiscord.MyDiscordClient.GetGuild(Options.DISCORD_GUILD_ID).GetTextChannel(Options.DISCORD_INTERNAL_CHANNEL).SendMessageAsync(MyLogMessage);
            }
            else
            {
                RUM = await SendMessage(Globals.BuildEmbed(null, MyLogMessage, null, pEmbedColor));
            }
            return RUM;
        }


        public static async Task<RestUserMessage> SendMessage(Embed pEmbed)
        {
            RestUserMessage RUM;
            RUM = await APIs.MyDiscord.MyDiscordClient.GetGuild(Options.DISCORD_GUILD_ID).GetTextChannel(Options.DISCORD_INTERNAL_CHANNEL).SendMessageAsync(embed: pEmbed);
            return RUM;
        }


        /// <summary>
        /// Event when DiscordClient is fully initialized and ready.
        /// </summary>
        /// <returns></returns>
        private Task ReadyAsync()
        {
            _ = Task.Run(async () =>
            {
                StringBuilder sb = new StringBuilder($"{MyDiscordClient.CurrentUser} is connected!");
                Helper.Logger.Log(sb.ToString(), 1);
                //await CommandHandling.MyCommandHandling.BuildCommands(APIs.MyDiscord.MyDiscordClient);

                //return Task.CompletedTask;
            });
            return Task.CompletedTask;
        }


        /// <summary>
        /// MyDiscordBot Event for any message in any channel.
        /// </summary>
        /// <param name="messageParam"></param>
        /// <returns></returns>
        private Task MessageReceivedAsync(SocketMessage messageParam)
        {
            _ = Task.Run(async () =>
            {
                if (messageParam.Channel.Id == Options.DISCORD_PUBLIC_CHANNEL ||
                messageParam.Channel.Id == Options.DISCORD_INTERNAL_CHANNEL)
                {
                    Logic.ProcessPotentialSong(messageParam, prevMessageParam);
                    prevMessageParam = messageParam;
                }
            });
            return Task.CompletedTask;
        }


        private Task MyDiscordClient_MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            _ = Task.Run(async () =>
            {
                MessageReceivedAsync(arg2);
                return Task.CompletedTask;
            });
            return Task.CompletedTask;
        }


        /// <summary>
        /// MyDiscordBot Event for any ADDED reaction to ANY message.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <returns></returns>
        private Task ReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if ((arg3.Channel.Id == Options.DISCORD_INTERNAL_CHANNEL) &&
                        (arg3.UserId != Globals.OurBotID))
                    {
                        Console.WriteLine("User: '{0}' reacted to message: '{1}' with reaction: '{2}'", arg3.User, arg3.MessageId, arg3.Emote.Name);

                        if (arg3.Emote.Equals(Globals.MyReactionEmote))
                        {
                            IUserMessage asdf = await arg1.DownloadAsync();

                            string SongName = "";
                            string SongUri = "";
                            // see if we can yeet song
                            foreach (Embed tmpEmbed in asdf.Embeds)
                            {
                                foreach (var sth in tmpEmbed.Fields)
                                {
                                    if (sth.Name == Globals.SongAddedTopline)
                                    {
                                        SongUri = sth.Value;
                                    }
                                    else if (sth.Name == Globals.SongAddedDescription)
                                    {
                                        SongName = sth.Value;
                                    }
                                }
                            }

                            List<KeyValuePair<string, string>> SongList = new List<KeyValuePair<string, string>>();
                            SongList.Add(new KeyValuePair<string, string>("Removed following Song from the Playlist, as per Reaction-Request:", SongName));

                            APIs.MySpotify.RemoveFromPlaylist(SongUri);
                            SendMessage(Globals.BuildEmbed(null, null, SongList, Globals.EmbedColors.LoggingEmbed));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Helper.Logger.Log(ex);
                }
            });
            return Task.CompletedTask;
        }


        /// <summary>
        /// MyDiscordBot Event for any SlashCommand.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private Task SlashCommandExecuted(SocketSlashCommand arg)
        {
            _ = Task.Run(async () =>
            {
                CommandHandling.MyCommandHandling.SlashCommandHandler(arg);
            });
            return Task.CompletedTask;
        }
    }
}
