using System;
using System.Threading.Tasks;
using System.IO;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace GGStriveUtilsBot
{
    class Program
    {
        static DiscordClient discord;
        static CommandsNextExtension commands;
        static SlashCommandsExtension slash;

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = System.IO.File.ReadAllText("token-testing.txt"),
                TokenType = TokenType.Bot,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug
            });

            discord.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromSeconds(60)
            });

            commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "?" }
            });

            slash = discord.UseSlashCommands();

            commands.RegisterCommands<Commands.FrameDataModule>();

            slash.RegisterCommands<SlashCommands.FrameDataSlashModule>(865166124568150026); //testing server

            //download frame data
            Utils.DustloopDataFetcher.Initialize();
            
            await discord.ConnectAsync(new DiscordActivity("Asuka R. Kreutz Radio Station", ActivityType.ListeningTo));
            await Task.Delay(-1);
        }
    }
}
