﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace GGStriveUtilsBot.SlashCommands
{
    class FrameDataSlashModule : ApplicationCommandModule
    {
        [SlashCommand("framedata", "Fetch frame data of a specified move from Dustloop wiki.")]
        public async Task FrameDataCommand(InteractionContext ctx, [Option("Request", "(Optional) Character name, followed by move name or numpad notation of it")] string move)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            //var result = await Utils.FrameDataEmbedBuilder.selectEmbed(ctx.Client, ctx.User, ctx.Channel, character.GetName() + " " + move);
            //if (result != null)
            //    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(result));
            var result = await Utils.FrameDataEmbedBuilder.selectEmbed(ctx.Client, ctx.User, ctx.Channel, move);
            if (result != null)
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(result));
            else
                await ctx.DeleteResponseAsync();
        }
    }
}
