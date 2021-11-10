using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Text;

namespace GGStriveUtilsBot.Utils {
    class CustomHelpFormatter : BaseHelpFormatter {

        protected DiscordEmbedBuilder _embed;
        public CustomHelpFormatter(CommandContext ctx) : base(ctx) {
            _embed = new DiscordEmbedBuilder();
            _embed.Color = DiscordColor.Violet;
        }

        public override BaseHelpFormatter WithCommand(Command command) {
            if (command.Name != "framedata") {
                return this;
            }
            _embed.WithTitle("Framedata Help");
            _embed.AddField("Specifying Character & Move", string.Join(
                "",
                "Moves can be specified in ",
                "[numpad notation](https://dustloop.com/wiki/index.php?title=Notation#Numpad_Notation) ",
                "or directly by name.\n",
                "Characters can be specified by full name, first name, or nickname. ",
                "If specifying a move by name, character may be omitted entirely.\n",
                "`!f sol 6S`\n",
                "`!f nago fukyo`\n",
                "`!f mortobato`\n"
            ));
            _embed.AddField("Rekka & Follow-up Moves", string.Join(
                "",
                "[Rekka Moves](https://glossary.infil.net/?t=Rekka) ",
                "are specified by each stage separated by spaces.\n",
                "`!f chipp 236S 236K`\n",
                "Follow-up moves are specified normally for the initial portion, ",
                "& additional attack buttons (in numpad notation) for the optional follow-up.\n",
                "`!f sol 236K`\n",
                "`!f sol 236KK`\n"
            ));
            _embed.AddField("Air, Charge, Backturn, & Level Moves", string.Join(
                "",
                "[Air Moves](https://dustloop.com/wiki/index.php?title=Notation#Air_Moves) ",
                "are denoted by the `j` prefix\n",
                "`!f May jH`\n",
                "[Charge Moves](https://dustloop.com/wiki/index.php?title=Notation#Charge_Moves) ",
                "are specified by `[x]` to indicate a held charge.\n",
                "`!f axl [4]6S`\n",
                "Leo's [backturn](https://dustloop.com/wiki/index.php?title=GGST/Leo_Whitefang#Brynhildr_Moves) ",
                "moves are specified by the `bt` prefix.\n",
                "`!f leo btH`\n",
                "Moves with multiple 'levels' are specified by appending \"`level ...`\" ",
                "to the move in numpad notation. \n",
                "`!f gold 236S level 2`\n",
                "`!f nago 5H level BR`\n"
            ));
            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> cmds) {
            _embed.WithTitle("Help");
            _embed.WithDescription("Available Commands");
            foreach (var cmd in cmds) {
                if (cmd.Name == "framedata") {
                    _embed.AddField(
                        cmd.Name + " (alias \"" + cmd.Aliases[0] + "\")",
                        string.Join(
                            "",
                            cmd.Description,
                            "\nTo see the expected format for framedata, use:\n",
                            "`!help f`\n"
                        )
                    );
                } else if (cmd.Name == "help") {
                    _embed.AddField(
                        cmd.Name,
                        string.Join(
                            "",
                            cmd.Description,
                            " You're using it right now.\n"
                        )
                    );
                }
            }
            return this;
        }
        
        public override CommandHelpMessage Build() {
            return new CommandHelpMessage(embed: _embed);
        }
    }

}