using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace GGStriveUtilsBot.Commands
{
    class UpdateDataModule : BaseCommandModule
    {
        [Command("updatedata"), Aliases("update"), Description("Update frame data with newest additions from Dustloop wiki."), RequireOwner]
        public async Task UpdateDataCommand(CommandContext ctx)
        {
            Utils.DustloopDataFetcher.Initialize();
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
        }
    }
}
