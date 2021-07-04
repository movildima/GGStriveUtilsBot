using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GGStriveUtilsBot
{
    static class GenericEmbedBuilder
    {
        private static DiscordEmbedBuilder defaultBuilder = new DiscordEmbedBuilder()
        {
            Footer = new DiscordEmbedBuilder.EmbedFooter()
            {
                IconUrl = "https://cdn.discordapp.com/avatars/861273965666238485/2287810a2e3e5498e3738188d047ebc6.webp?size=64",
                Text = "Data provided by the Dustloop wiki"
            },
        };

        public static DiscordEmbedBuilder Create()
        {
            var r = new DiscordEmbedBuilder(defaultBuilder);

            //todo: colors and other customization

            return r;
        }
    }
}
