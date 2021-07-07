using System;
using System.Threading.Tasks;
using System.IO;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

namespace GGStriveUtilsBot
{
    class Program
    {
        static DiscordClient discord;
        static CommandsNextExtension commands;

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = System.IO.File.ReadAllText("token.txt"),
                TokenType = TokenType.Bot
            });

            discord.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromSeconds(60)
            });

            commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "!" }
            });

            commands.RegisterCommands<Commands.FrameDataModule>();

            //download frame data
            Utils.DustloopDataFetcher.Initialize();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
