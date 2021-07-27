using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace GGStriveUtilsBot.Utils
{
    static class FrameDataEmbedBuilder
    {
        private static readonly string pageLink = "https://www.dustloop.com/wiki/index.php?title=GGST/{0}#{1}";

        public static async Task<DiscordEmbed> selectEmbed(DiscordClient client, DiscordUser user, DiscordChannel channel, string Move)
        {
            (Character? character, string move, bool isNumpad) = Utils.InputParser.parseFrameDataInput(Move);

            var results = Utils.DustloopDataFetcher.fetchMove(character, move, isNumpad);
            var interactivity = client.GetInteractivity();

            switch (results.result)
            {
                case MoveDataResult.Success:
                    return build1XEmbed(results.moves[0]);
                case MoveDataResult.NoResult:
                    return buildNoResultEmbed();
                case MoveDataResult.ExtraResults:
                    var response = await channel.SendMessageAsync(buildXEmbed(results.moves));
                    for (int i = 0; i < results.moves.Count; i++)
                    {
                        if(i < emoteList.Count)
                        {
                            await response.CreateReactionAsync(DiscordEmoji.FromUnicode(client, emoteList[i]));
                        }
                    }
                    var reaction = await interactivity.WaitForReactionAsync(f => f.User == user && emoteList.Where(g => f.Emoji == DiscordEmoji.FromUnicode(g)).Count() > 0, TimeSpan.FromSeconds(20));
                    if (!reaction.TimedOut)
                    {
                        await response.DeleteAsync();
                        return build1XEmbed(results.moves[emoteList.IndexOf(emoteList.Where(g => reaction.Result.Emoji == DiscordEmoji.FromUnicode(g)).FirstOrDefault())]);
                    }
                    else
                    {
                        await response.DeleteAsync();
                        return null;
                    }
                case MoveDataResult.TooManyResults:
                    return buildTooManyEmbed();
            };
            return null;
        }
        private static DiscordEmbed build1XEmbed(MoveData move)
        {
            var embed = GenericEmbedBuilder.Create();
            embed.Author = new DiscordEmbedBuilder.EmbedAuthor();
            if (Utils.DustloopDataFetcher.iconSource.Where(f => f.name == move.chara && f.iconLoaded).Count() == 1)
            {
                embed.Author.IconUrl = Utils.DustloopDataFetcher.iconSource.Where(f => f.name == move.chara).FirstOrDefault().iconFull;
                embed = embed.WithThumbnail(Utils.DustloopDataFetcher.iconSource.Where(f => f.name == move.chara).FirstOrDefault().iconFull);
            }
            if (!string.IsNullOrEmpty(move.name))
            {
                embed.Author.Name = "Frame data for " + move.name;
                embed.Author.Url = string.Format(pageLink, move.chara, move.name).Replace(" ", "_");
            }
            else
            {
                embed.Author.Name = "Frame data for " + move.input;
                embed.Author.Url = string.Format(pageLink, move.chara, move.input).Replace(" ", "_");
            }
            if (!string.IsNullOrEmpty(move.input))
                embed.AddField("Input", move.input, true);
            if (!string.IsNullOrEmpty(move.damage))
                embed.AddField("Damage", move.damage, true);
            if (!string.IsNullOrEmpty(move.guard))
                embed.AddField("Guard", move.guard, true);
            if (!string.IsNullOrEmpty(move.startup))
                embed.AddField("Startup", move.startup, true);
            if (!string.IsNullOrEmpty(move.active))
                embed.AddField("Active", move.active, true);
            if (!string.IsNullOrEmpty(move.recovery))
                embed.AddField("Recovery", move.recovery, true);
            if (!string.IsNullOrEmpty(move.onBlock))
                embed.AddField("On block", move.onBlock, true);
            if (!string.IsNullOrEmpty(move.onHit))
                embed.AddField("On hit", move.onHit, true);
            if (!string.IsNullOrEmpty(move.invuln))
                embed.AddField("Invuln", move.invuln, true);
            if (move.imgLoaded)
                embed = embed.WithThumbnail(move.imgFull);

            return embed.Build();
        }

        public static DiscordEmbed buildXEmbed(List<MoveData> moves)
        {
            var embed = GenericEmbedBuilder.Create();
            embed = embed.WithTitle("Multiple results found!");
            embed = embed.WithDescription("Select one of the following moves:");
            var sameChara = moves.Where(f => f.chara == moves[0].chara).Count() == moves.Count;
            foreach (var move in moves)
            {
                if (sameChara)
                    embed.AddField((moves.IndexOf(move) + 1).ToString() + ": " + move.name, string.IsNullOrEmpty(move.input) ? "No input" : move.input);
                else
                    embed.AddField((moves.IndexOf(move) + 1).ToString() + ": (" + move.chara + ") " + move.name, string.IsNullOrEmpty(move.input) ? "No input" : move.input);
            }
            return embed.Build();
        }

        public static DiscordEmbed buildNoResultEmbed()
        {
            var embed = GenericEmbedBuilder.Create();
            embed = embed.WithTitle("No results found!");
            embed = embed.WithDescription("Double check your request for errors.");
            return embed.Build();
        }

        public static DiscordEmbed buildTooManyEmbed()
        {
            var embed = GenericEmbedBuilder.Create();
            embed = embed.WithTitle("Too many results found!");
            embed = embed.WithDescription("Please make a more specific request.");
            return embed.Build();
        }

        public static List<string> emoteList = new List<string>()
            {
                "1️⃣",
                "2️⃣",
                "3️⃣",
                "4️⃣",
                "5️⃣",
                "6️⃣",
                "7️⃣",
                "8️⃣"
            };
    }
}
