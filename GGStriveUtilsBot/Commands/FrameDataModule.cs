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
        public async Task FrameDataCommand(CommandContext ctx, [RemainingText, Description("Character name followed by move, or just a move name(move can be indicated by name or in numpad notation)")] string Move)
        {
            await ctx.TriggerTypingAsync();

            var result = await Utils.FrameDataEmbedBuilder.selectEmbed(ctx.Client, ctx.User, ctx.Channel, Move);
            if(result != null)
            {
                await ctx.RespondAsync(result);
            }
        }
    }
}
