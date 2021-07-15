using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GGStriveUtilsBot
{
    static class GenericEmbedBuilder
    {
        private static DiscordEmbedBuilder defaultBuilder = new DiscordEmbedBuilder();

        public static DiscordEmbedBuilder Create(bool dustloopFooter = true)
        {
            var r = new DiscordEmbedBuilder(defaultBuilder);
            r.Footer = new DiscordEmbedBuilder.EmbedFooter();
            r.Footer.IconUrl = "https://cdn.discordapp.com/avatars/861273965666238485/2287810a2e3e5498e3738188d047ebc6.webp?size=64";
            if (dustloopFooter)
                r.Footer.Text = "Data provided by Dustloop wiki";
            else
                r.Footer.Text = "I did the Dragon Uninstall.";

            //todo: colors and other customization

            return r;
        }
    }
}
