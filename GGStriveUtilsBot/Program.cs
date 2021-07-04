using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;

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
                Token = "Token",
                TokenType = TokenType.Bot
            });

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
