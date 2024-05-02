using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace GGStriveUtilsBot.Utils
{
    static class FrameDataEmbedBuilder
    {
        private static readonly string pageLink = "https://www.dustloop.com/wiki/index.php?title=GGST/{0}#{1}";

        public static async Task<DiscordEmbed> selectEmbed(DiscordClient client, DiscordUser user, DiscordChannel channel, string Move)
        {
            (Character? character, string move, string level, bool isNumpad) = Utils.InputParser.parseFrameDataInput(Move);

            var results = DustloopDataFetcher.fetchMove(character, move, level, isNumpad);
            var interactivity = client.GetInteractivity();
            DiscordMessage buttonResponse;
            InteractivityResult<ComponentInteractionCreateEventArgs> buttonReaction;

            switch (results.result)
            {
                case MoveDataResult.Success:
                    return build1XEmbed(results.moves[0]);
                case MoveDataResult.NoResult:
                    await Commands.ErrorChannel.sendMessage(results, Move, client, user);
                    return buildNoResultEmbed();
                case MoveDataResult.ExtraResults:
                    //var response = await channel.SendMessageAsync(buildXEmbed(results.moves));
                    //for (int i = 0; i < results.moves.Count; i++)
                    //{
                    //    if (i < emoteList.Count)
                    //    {
                    //        await response.CreateReactionAsync(DiscordEmoji.FromUnicode(client, emoteList[i]));
                    //    }
                    //}
                    //var reaction = await interactivity.WaitForReactionAsync(f => f.User == user && emoteList.Where(g => f.Emoji == DiscordEmoji.FromUnicode(g)).Count() > 0, TimeSpan.FromSeconds(20));
                    //if (!reaction.TimedOut)
                    //{
                    //    await response.DeleteAsync();
                    //    return build1XEmbed(results.moves[emoteList.IndexOf(emoteList.Where(g => reaction.Result.Emoji == DiscordEmoji.FromUnicode(g)).FirstOrDefault())]);
                    //}
                    //else
                    //{
                    //    await response.DeleteAsync();
                    //    return null;
                    //}
                    buttonResponse = await channel.SendMessageAsync(buildXEmbed(results.moves));
                    buttonReaction = await buttonResponse.WaitForButtonAsync(user, TimeSpan.FromSeconds(40));
                    if (!buttonReaction.TimedOut)
                    {
                        await buttonResponse.DeleteAsync();
                        return build1XEmbed(results.moves.ElementAt(int.Parse(buttonReaction.Result.Id)));
                    }
                    else
                    {
                        await buttonResponse.DeleteAsync();
                        return null;
                    }
                case MoveDataResult.TooManyResults:
                    await Commands.ErrorChannel.sendMessage(results, Move, client, user);
                    return buildTooManyEmbed();
                case MoveDataResult.SpecialBehemoth:
                    buttonResponse = await channel.SendMessageAsync(buildBehemothSelector(false));
                    buttonReaction = await buttonResponse.WaitForButtonAsync(user, TimeSpan.FromSeconds(40));
                    if (!buttonReaction.TimedOut)
                    {
                        if (buttonReaction.Result.Id == "air_ok")
                        {
                            await buttonResponse.ModifyAsync(buildBehemothSelector(true));
                            await buttonReaction.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                            buttonReaction = await buttonResponse.WaitForButtonAsync(user, TimeSpan.FromSeconds(40));
                            if (!buttonReaction.TimedOut)
                            {
                                await buttonResponse.DeleteAsync();
                                return build1XEmbed(results.moves.FirstOrDefault(f => f.input == buttonReaction.Result.Id));
                            }
                            else
                            {
                                await buttonResponse.DeleteAsync();
                                return null;
                            }
                        }
                        else
                        {
                            await buttonResponse.DeleteAsync();
                            return build1XEmbed(results.moves.FirstOrDefault(f => f.input == buttonReaction.Result.Id));
                        }
                    }
                    else
                    {
                        await buttonResponse.DeleteAsync();
                        return null;
                    }
            };
            return null;
        }
        private static DiscordEmbed build1XEmbed(MoveData move)
        {
            var embed = GenericEmbedBuilder.Create();
            embed.Author = new DiscordEmbedBuilder.EmbedAuthor();
            if (DustloopDataFetcher.iconSource.Where(f => f.name == move.chara && f.iconLoaded).Count() == 1)
            {
                var iconSource = DustloopDataFetcher.iconSource.FirstOrDefault(f => f.name == move.chara).iconFull;
                embed.Author.IconUrl = iconSource;
                embed = embed.WithThumbnail(iconSource);
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
                embed.AddField("Damage", move.damage.Replace("*", "\\*"), true);
            if (!string.IsNullOrEmpty(move.guard))
                embed.AddField("Guard", move.guard.Replace("*", "\\*"), true);
            if (!string.IsNullOrEmpty(move.startup))
                embed.AddField("Startup", move.startup.Replace("*", "\\*"), true);
            if (!string.IsNullOrEmpty(move.active))
                embed.AddField("Active", move.active.Replace("*", "\\*"), true);
            if (!string.IsNullOrEmpty(move.recovery))
                embed.AddField("Recovery", move.recovery.Replace("*", "\\*"), true);
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

        public static DiscordMessageBuilder buildXEmbed(List<MoveData> moves)
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

            DiscordComponent[] components1 = new DiscordComponent[4];
            DiscordComponent[] components2 = new DiscordComponent[4];
            bool big = false;
            for (int i = 0; i < moves.Count; i++)
            {
                if (i < 4)
                    components1[i] = new DiscordButtonComponent(ButtonStyle.Secondary, i.ToString(), "", false, new DiscordComponentEmoji(emoteList[i]));
                else
                {
                    components2[i - 4] = new DiscordButtonComponent(ButtonStyle.Secondary, i.ToString(), "", false, new DiscordComponentEmoji(emoteList[i]));
                    big = true;
                }
            }
            var message = new DiscordMessageBuilder()
                .WithEmbed(embed)
                .AddComponents(components1);
            if(big)
                message.AddComponents(components2);
            return message;
        }

        public static DiscordEmbed buildNoResultEmbed()
        {
            var embed = GenericEmbedBuilder.Create();
            embed = embed.WithTitle("No results found!");
            embed = embed.WithDescription(
                String.Join(
                    "",
                    "Double check your request for errors.\n"
                )
            );
            return embed.Build();
        }

        public static DiscordEmbed buildTooManyEmbed()
        {
            var embed = GenericEmbedBuilder.Create();
            embed = embed.WithTitle("Too many results found!");
            embed = embed.WithDescription("Please make a more specific request.");
            return embed.Build();
        }

        public static DiscordMessageBuilder buildBehemothSelector(bool buildAir)
        {
            DiscordMessageBuilder goldlewisGroundMsg = new DiscordMessageBuilder()
            /*embed*/
            .WithEmbed(GenericEmbedBuilder.Create()
                        .WithImageUrl("https://cdn.discordapp.com/attachments/1036038892022927450/1036039026278404136/Behemoth_Typhoon___DPAD_Ver.png") /* PseudoWoodo's awesome behemoth selector \o/ */
                        .WithAuthor("In this Behemoth Typhoon", "https://www.dustloop.com/wiki/index.php?title=GGST/Goldlewis_Dickinson#Note_on_Behemoth_Typhoon",
                                    DustloopDataFetcher.iconSource.Where(f => f.name == "Goldlewis Dickinson" && f.iconLoaded).Count() == 1 ? DustloopDataFetcher.iconSource.FirstOrDefault(f => f.name == "Goldlewis Dickinson").iconFull : null)
                        .WithDescription("You select which version of Behemoth Typhoon you want to check.\nCurrently selecting ground moves."))
            // the whole shebang lmao
            .AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Secondary, "empty1", "", true,  new DiscordComponentEmoji(1036029682354761808)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "87412H", "", false, new DiscordComponentEmoji(1036013167496663190)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "89632H", "", false, new DiscordComponentEmoji(1036013178653524039)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "empty2", "", true,  new DiscordComponentEmoji(1036029682354761808))
            })
            .AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Secondary, "47896H", "", false, new DiscordComponentEmoji(1036013106280792064)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "empty3", "", true,  new DiscordComponentEmoji(1036029682354761808)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "empty4", "", true,  new DiscordComponentEmoji(1036029682354761808)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "69874H", "", false, new DiscordComponentEmoji(1036013150828503132))
            })
            .AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Secondary, "41236H", "", false, new DiscordComponentEmoji(1036013094670979102)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "empty5", "", true,  new DiscordComponentEmoji(1036029682354761808)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "empty6", "", true,  new DiscordComponentEmoji(1036029682354761808)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "63214H", "", false, new DiscordComponentEmoji(1036013122143649813))
            })
            .AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Secondary, "empty7", "", true,  new DiscordComponentEmoji(1036029682354761808)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "21478H", "", false, new DiscordComponentEmoji(1036013073699438652)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "23698H", "", false, new DiscordComponentEmoji(1036013082750746704)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "empty8", "", true,  new DiscordComponentEmoji(1036029682354761808))
            })
            .AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Success, "air_ok", "Switch to air moves", false)
            });

            DiscordMessageBuilder goldlewisAirMsg = new DiscordMessageBuilder()
            /*embed*/
            .WithEmbed(GenericEmbedBuilder.Create()
                        .WithImageUrl("https://cdn.discordapp.com/attachments/1036038892022927450/1036039026278404136/Behemoth_Typhoon___DPAD_Ver.png") /* PseudoWoodo's awesome behemoth selector \o/ */
                        .WithAuthor("In this Behemoth Typhoon", "https://www.dustloop.com/wiki/index.php?title=GGST/Goldlewis_Dickinson#Note_on_Behemoth_Typhoon",
                                    DustloopDataFetcher.iconSource.Where(f => f.name == "Goldlewis Dickinson" && f.iconLoaded).Count() == 1 ? DustloopDataFetcher.iconSource.FirstOrDefault(f => f.name == "Goldlewis Dickinson").iconFull : null)
                        .WithDescription("You select which version of Behemoth Typhoon you want to check.\nCurrently selecting air moves."))
            // the whole shebang lmao
            .AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Secondary, "empty1", "", true,  new DiscordComponentEmoji(1036029682354761808)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "j.87412H", "", false, new DiscordComponentEmoji(1036013167496663190)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "j.89632H", "", false, new DiscordComponentEmoji(1036013178653524039)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "empty2", "", true,  new DiscordComponentEmoji(1036029682354761808))
            })
            .AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Secondary, "j.47896H", "", false, new DiscordComponentEmoji(1036013106280792064)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "empty3", "", true,  new DiscordComponentEmoji(1036029682354761808)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "empty4", "", true,  new DiscordComponentEmoji(1036029682354761808)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "j.69874H", "", false, new DiscordComponentEmoji(1036013150828503132))
            })
            .AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Secondary, "j.41236H", "", false, new DiscordComponentEmoji(1036013094670979102)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "empty5", "", true,  new DiscordComponentEmoji(1036029682354761808)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "empty6", "", true,  new DiscordComponentEmoji(1036029682354761808)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "j.63214H", "", false, new DiscordComponentEmoji(1036013122143649813))
            })
            .AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Secondary, "empty7", "", true,  new DiscordComponentEmoji(1036029682354761808)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "j.21478H", "", false, new DiscordComponentEmoji(1036013073699438652)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "j.23698H", "", false, new DiscordComponentEmoji(1036013082750746704)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "empty8", "", true,  new DiscordComponentEmoji(1036029682354761808))
            })
            .AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Success, "air_ok", "Air moves enabled", true)
            });

            if (!buildAir)
            {
                return goldlewisGroundMsg;
            }
            else
            {
                return goldlewisAirMsg;
            }
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
