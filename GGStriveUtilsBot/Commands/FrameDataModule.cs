using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.Net;
using System.IO;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace GGStriveUtilsBot.Commands
{
    class FrameDataModule : BaseCommandModule
    {

        [Command("framedata"), Aliases("f"), Description("Fetch frame data of a specified move from Dustloop wiki.")]
        public async Task FrameDataCommand(CommandContext ctx, [RemainingText, Description("TBD")] string Move)
        {
            await ctx.TriggerTypingAsync();

            (string chara, string move, bool isNumpad) = Utils.InputParser.parseFrameDataInput(Move);
            var results = Utils.DustloopDataFetcher.fetchMove(chara, move);
            var interactivity = ctx.Client.GetInteractivity();
            DiscordEmoji emoji1 = DiscordEmoji.FromName(ctx.Client, ":one:");
            DiscordEmoji emoji2 = DiscordEmoji.FromName(ctx.Client, ":two:");
            DiscordEmoji emoji3 = DiscordEmoji.FromName(ctx.Client, ":three:");
            DiscordEmoji emoji4 = DiscordEmoji.FromName(ctx.Client, ":four:");

            switch (results.result)
            {
                case MoveDataResult.Success:
                    await ctx.Channel.SendMessageAsync(build1XEmbed(results.moves[0]));
                    break;
                case MoveDataResult.NoResult:
                    await ctx.Channel.SendMessageAsync(buildNoResultEmbed());
                    break;
                case MoveDataResult.ExtraResults:
                    var response = await ctx.RespondAsync(buildXEmbed(results.moves));
                    for (int i = 1; i <= results.moves.Count; i++)
                    {
                        if (i == 1)
                            await response.CreateReactionAsync(emoji1);
                        if (i == 2)
                            await response.CreateReactionAsync(emoji2);
                        if (i == 3)
                            await response.CreateReactionAsync(emoji3);
                        if (i == 4)
                            await response.CreateReactionAsync(emoji4);
                    }
                    var reaction = await interactivity.WaitForReactionAsync(f => f.User == ctx.User && (f.Emoji == emoji1 || f.Emoji == emoji2 || f.Emoji == emoji3 || f.Emoji == emoji4), TimeSpan.FromSeconds(20));
                    if (!reaction.TimedOut)
                    {
                        await response.DeleteAsync();
                        if (reaction.Result.Emoji == emoji1)
                            await response.Channel.SendMessageAsync(build1XEmbed(results.moves[0]));
                        if (reaction.Result.Emoji == emoji2)
                            await response.Channel.SendMessageAsync(build1XEmbed(results.moves[1]));
                        if (reaction.Result.Emoji == emoji3)
                            await response.Channel.SendMessageAsync(build1XEmbed(results.moves[2]));
                        if (reaction.Result.Emoji == emoji4)
                            await response.Channel.SendMessageAsync(build1XEmbed(results.moves[3]));
                    }
                    else
                        await response.DeleteAsync();
                    break;
                case MoveDataResult.TooManyResults:
                    await ctx.RespondAsync(buildTooManyEmbed());
                    break;
            };
        }

        private DiscordEmbed build1XEmbed(MoveData move)
        {
            var embed = GenericEmbedBuilder.Create();

            if (!string.IsNullOrEmpty(move.name))
                embed = embed.WithTitle("Frame data for " + move.name);
            else
                embed = embed.WithTitle("Frame data for " + move.input);
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
            embed = embed.WithThumbnail(move.imgFull);

            return embed.Build();
        }

        private DiscordEmbed buildXEmbed(List<MoveData> moves)
        {
            var embed = GenericEmbedBuilder.Create();
            embed = embed.WithTitle("Multiple results found!");
            embed = embed.WithDescription("Select one of the following moves:");
            foreach (var move in moves)
            {
                embed.AddField((moves.IndexOf(move) + 1).ToString() + ": " + move.name, move.input);
            }
            return embed.Build();
        }

        private DiscordEmbed buildNoResultEmbed()
        {
            var embed = GenericEmbedBuilder.Create();
            embed = embed.WithTitle("No results found!");
            embed = embed.WithDescription("Double check your request for errors.");
            return embed.Build();
        }

        private DiscordEmbed buildTooManyEmbed()
        {
            var embed = GenericEmbedBuilder.Create();
            embed = embed.WithTitle("Too many results found!");
            embed = embed.WithDescription("Please make a more specific request.");
            return embed.Build();
        }
    }
}
