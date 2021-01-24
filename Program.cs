using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace HypixelStatsBot
{
    public class Program
    {
        public static void Main(string[] args) 
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient client;
        private CommandHandler commandHandler;

        public async Task MainAsync()
        {
            client = new DiscordSocketClient();
            client.Log += Log;

            string token = Constants.DiscordAPIKey;

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            commandHandler = new CommandHandler(client, new CommandService());
            await commandHandler.InstallCommandsAsync();

            await Task.Delay(-1);
        }

        private Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());

            return Task.CompletedTask;
        }
    }
}
