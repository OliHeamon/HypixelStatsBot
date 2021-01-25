using Discord.Commands;
using Discord.WebSocket;
using HypixelStatsBot.Constant;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace HypixelStatsBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly IServiceProvider services;

        public CommandHandler(IServiceProvider services)
        {
            this.services = services;

            commands = services.GetRequiredService<CommandService>();
            client = services.GetRequiredService<DiscordSocketClient>();
        }

        public async Task InstallCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;

            await commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: services);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            if (!(messageParam is SocketUserMessage message))
            {
                return;
            }

            int argPos = 0;

            if (!(message.HasCharPrefix(Constants.Prefix, ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos)) || message.Author.IsBot)
            {
                return;
            }

            SocketCommandContext context = new SocketCommandContext(client, message);

            await commands.ExecuteAsync(context: context, argPos: argPos, services: services);
        }
    }
}
