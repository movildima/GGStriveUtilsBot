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
        public async Task FrameDataCommand(CommandContext ctx, string parameters)
        {
            var builder = GenericEmbedBuilder.Create();
            builder.Title = "Anji Mito";
            builder.Description = "Fuujin frame data";
            builder.AddField("Input", "236HS", true);
            builder.AddField("Damage", "35", true);
            builder.AddField("Guard", "All", true);
            builder.AddField("Startup", "17[33]", true);
            builder.AddField("Active", "4", true);
            builder.AddField("Recovery", "29", true);
            builder.AddField("On block", "-16", true);
            builder.AddField("On hit", "-13", true);
            builder.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail();
            builder.Thumbnail.Url = "https://dustloop.com/wiki/images/thumb/a/a0/GGST_Anji_Mito_Fuujin.png/651px-GGST_Anji_Mito_Fuujin.png";
            await ctx.RespondAsync(builder.Build());
        }
    }
}
