using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace GGStriveUtilsBot.Commands
{
    class FrameDataModule : BaseCommandModule
    {
        [Command("framedata"), Aliases("f")]
        public async Task FrameDataCommand(CommandContext ctx, string Character, [RemainingText] string Move)
        {
            (string chara, string move) = Utils.InputParser.parseFrameDataInput(Character + " " + Move);
            var results = Utils.DustloopDataFetcher.fetchMove(chara, move);
            ctx.RespondAsync(buildEmbed(results));
        }

        [Command("framedata")]
        public async Task FrameDataCommand(CommandContext ctx, string Move)
        {
            var results = Utils.DustloopDataFetcher.fetchMove(null, Move);
            await ctx.RespondAsync(buildEmbed(results));
        }

        private DiscordEmbed buildEmbed(MoveListInternal list)
        {
            switch (list.result)
            {
                case MoveDataResult.Success:
                    return build1XEmbed(list.moves[0]);
                case MoveDataResult.NoResult:
                    var embed = GenericEmbedBuilder.Create();
                    return embed.Build();
                case MoveDataResult.ExtraResults:
                    return build3XEmbed(list.moves);
                case MoveDataResult.TooManyResults:
                    var embed2 = GenericEmbedBuilder.Create();
                    return embed2.Build();
                default:
                    var embed3 = GenericEmbedBuilder.Create();
                    return embed3.Build();
            };
        }

        private DiscordEmbed build1XEmbed(MoveData move)
        {
            var embed = GenericEmbedBuilder.Create();

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
            embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail();
            embed.Thumbnail.Url = move.images[0];

            return embed.Build();
        }

        private DiscordEmbed build3XEmbed(List<MoveData> moves)
        {
            var embed = GenericEmbedBuilder.Create();
            return embed.Build();
        }
    }
}
