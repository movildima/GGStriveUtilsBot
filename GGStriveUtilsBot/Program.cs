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
#if !DEBUG
                Token = System.IO.File.ReadAllText("token.txt"),

                Intents = DiscordIntents.AllUnprivileged,
#elif DEBUG
                Token = System.IO.File.ReadAllText("token-testing.txt"),

                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
#endif
                TokenType = TokenType.Bot,

                //MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Trace
            });

            discord.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromSeconds(60)
            });

            commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
#if !DEBUG
                StringPrefixes = new[] { "!" }
#elif DEBUG
                StringPrefixes = new[] { "?" }
#endif
            });

            commands.SetHelpFormatter<Utils.CustomHelpFormatter>();

            slash = discord.UseSlashCommands();

            commands.RegisterCommands<Commands.FrameDataModule>();
            commands.RegisterCommands<Commands.AdminUtilsModule>();
            
            slash.RegisterCommands<SlashCommands.FrameDataSlashModule>();
            //ly liske
            discord.MessageCreated += async (s, e) =>
            {
                //Console.WriteLine("message: " + e.Message.Content);
                if (e.Message.Content == "Ly")
                    await e.Message.RespondAsync("Liske <:Ky:810597089980448768>");
            };

            //connect
            await discord.ConnectAsync(new DiscordActivity("Asuka R. Kreutz Radio Station", ActivityType.ListeningTo));
            slash.RefreshCommands();

            //download frame data
            await Utils.DustloopDataFetcher.Initialize();

            //load bad requests channel
            if (File.Exists("bad-requests.txt"))
            {
                Commands.ErrorChannel.isSet = true;
                Commands.ErrorChannel.channel = await discord.GetChannelAsync(Convert.ToUInt64(System.IO.File.ReadAllText("bad-requests.txt")));
            }

            await Task.Delay(-1);
        }
    }
}
