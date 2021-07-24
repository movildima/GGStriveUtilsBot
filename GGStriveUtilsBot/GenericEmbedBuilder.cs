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
            if (dustloopFooter)
            {
                r.Footer.IconUrl = "https://cdn.discordapp.com/icons/577242028497436672/e3cdd493a75785f290f1817951badaad.webp?size=64";
                r.Footer.Text = "Data provided by Dustloop wiki";
            }
            else
                r.Footer.Text = "I did the Dragon Uninstall.";

            //todo: colors and other customization

            return r;
        }
    }
}
