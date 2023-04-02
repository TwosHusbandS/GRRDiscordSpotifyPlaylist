using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WasIchHoerePlaylist.Helper
{
    internal class DiscordHelper
    {
        /// <summary>
        /// Gets Text for UI from a SocketGuildUser
        /// </summary>
        /// <param name="SGU"></param>
        /// <returns></returns>
        public static string GetUserTextFromSGU(SocketGuildUser SGU)
        {
            return SGU.Nickname + " [" + SGU.Username + "#" + SGU.Discriminator + "]";
        }


        /// <summary>
        /// Enum of our EmbedColors
        /// </summary>
        public enum EmbedColors
        {
            NormalEmbed,
            LoggingEmbed,
            ErrorEmbed,
            NoEmbed
        }


        /// <summary>
        /// Building an Embed from Parameters
        /// </summary>
        /// <param name="command"></param>
        /// <param name="pDescription"></param>
        /// <param name="pFields"></param>
        /// <param name="pEmbedColor"></param>
        /// <returns></returns>
        public static Embed BuildEmbed(SocketSlashCommand command, string pDescription, List<KeyValuePair<string, string>> pFields, EmbedColors pEmbedColor)
        {
            Color _color;


            // pEmbedColor could be no embed here, but tbh we dont give a rats ass, put it as Info i guess?
            switch (pEmbedColor)
            {
                case EmbedColors.ErrorEmbed:
                    _color = new Color(Options.ERROR_COLOR.R, Options.ERROR_COLOR.G, Options.ERROR_COLOR.B);
                    break;
                case EmbedColors.LoggingEmbed:
                    _color = new Color(Options.LOG_COLOR.R, Options.LOG_COLOR.G, Options.LOG_COLOR.B);
                    break;
                default:
                    _color = new Color(Options.EMBED_COLOR.R, Options.EMBED_COLOR.G, Options.EMBED_COLOR.B);
                    break;
            }

            var embed = new EmbedBuilder();

            embed.WithColor(_color);

            if (command != null)
            {
                embed.WithAuthor(command.User);
            }

            embed.WithTitle("#WasIchHöre - Spotify Bot")
                .WithUrl(Globals.SpotifyPlaylist); // url is on title

            if (!string.IsNullOrEmpty(pDescription))
            {
                embed.WithDescription(pDescription);
            }

            if (!(pFields == null))
            {
                foreach (var field in pFields)
                {
                    embed.AddField(field.Key, field.Value);
                }
            }

            embed.WithFooter(footer => footer.Text = "GermanRapReddit Discord Spotify Bot") // no markdown here
                .WithCurrentTimestamp();

            Embed myEmbed = embed.Build();

            return myEmbed;
        }

    }
}
