using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace GGStriveUtilsBot.Commands
{
    class AdminUtilsModule : BaseCommandModule
    {
        [Command("updatedata"), Aliases("update"), Description("Update frame data with newest additions from Dustloop wiki."), RequireOwner]
        public async Task UpdateDataCommand(CommandContext ctx)
        {
            Utils.DustloopDataFetcher.Initialize();
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
        }

        [Command("seterrorchannel"), Description("Set a channel where bad requests will be posted for troubleshooting."), RequireOwner]
        public async Task SetErrorChannelCommand(CommandContext ctx)
        {
            ErrorChannel.channel = ctx.Channel;
            ErrorChannel.isSet = true;
            await System.IO.File.WriteAllTextAsync("bad-requests.txt", ctx.Channel.Id.ToString());
            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder().WithDescription("This channel will now receive reports of bad requests."));
        }
    }

    public static class ErrorChannel
    {
        public static bool isSet { get; set; } = false;
        public static DiscordChannel channel { get; set; }

        public static async Task sendMessage(MoveListInternal moveList, string moveRequest, DiscordClient client, DiscordUser user)
        {
            if(isSet)
            {
                await client.SendMessageAsync(channel, new DiscordEmbedBuilder()
                    .WithAuthor("Bad request", null, "https://cdn.discordapp.com/avatars/861273965666238485/67e04e91354f7e00a9d3c44d16135193.webp?size=64")
                    .AddField("Requested on:", DateTime.Now.ToString(), true)
                    .AddField("User:", user.Username + "#" + user.Discriminator, true)
                    .AddField("Request string:", moveRequest)
                    .AddField("Results count:", moveList.moves.Count.ToString())
#if !DEBUG
                    .WithColor(new DiscordColor("2F1C1D")));
#elif DEBUG
                    .WithColor(DiscordColor.HotPink));
#endif
            }
        }
    }
}
