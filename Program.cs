using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HypixelStatsBot.Constant;
using HypixelStatsBot.Database;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace HypixelStatsBot
{
    public class Program
    {
        public static void Main(string[] args) 
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient client;

        public async Task MainAsync()
        {
            ServiceProvider services = new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddDbContext<PlayerStorage>()
                .BuildServiceProvider();

            client = services.GetRequiredService<DiscordSocketClient>();
            client.Log += Log;

            string token = Constants.DiscordAPIKey;

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await services.GetRequiredService<CommandHandler>().InstallCommandsAsync();

            await Task.Delay(-1);
        }

        private Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());

            return Task.CompletedTask;
        }
    }
}
