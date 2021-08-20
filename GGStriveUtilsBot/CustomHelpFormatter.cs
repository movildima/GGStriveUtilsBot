using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;

namespace GGStriveUtilsBot {
    public class CustomHelpFormatter: BaseHelpFormatter {

        protected DiscordEmbedBuilder _embed;

        public CustomHelpFormatter(CommandContext ctx) : base(ctx) {
            _embed = new DiscordEmbedBuilder();
        }

        public override BaseHelpFormatter WithCommand(Command command) {
            //_embed.AddField(command.Name, command.Description);
            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands) {
            //foreach (var cmd in subcommands) {
            //    _embed.AddField(cmd.Name, cmd.Description);
            //}
            return this;
        }

        public override CommandHelpMessage Build() {
            _embed.Color = DiscordColor.Rose;
            _embed.ClearFields();

            String title = "Help";
            _embed.WithTitle(title);

            String description = String.Join("",
                "Fetch frame-data for Guilty Gear Strive, ",
                "courtesy of the Dustloop Wiki!",
                "\n\n",
                "Enter `!help framedata` to see ",
                "arguments and example usage.");
            _embed.WithDescription(description);

            _embed.AddField("Commands", "`framedata`");

            CommandHelpMessage msg = new CommandHelpMessage(embed: _embed);
            return msg;
        }
    }
}
