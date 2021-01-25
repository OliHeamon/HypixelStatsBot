using Discord;
using Discord.Commands;
using HypixelStatsBot.Database;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static HypixelStatsBot.Constant.Constants;

namespace HypixelStatsBot.Module
{
    public class BedwarsModule : ModuleBase
    {
        private readonly PlayerStorage database;

        public BedwarsModule(IServiceProvider services) 
        {
            database = services.GetRequiredService<PlayerStorage>();
        }

        [Command("addentry")]
        [Summary("Adds a database entry; usage is !addentry [id].")]
        public async Task AddEntry([Remainder] string input)
        {
            string[] parameters = input.Split(' ');

            List<AccountData> data = await database.AccountData.ToListAsync();

            if (data.Any(a => a.DiscordID == parameters[0]))
            {
                await ReplyAsync("User not added (already in database).");
                return;
            }

            try
            {
                if (!IsValidId(parameters[0]))
                {
                    throw new ArgumentException("Invalid user ID.");
                }

                await database.AddAsync(new AccountData() 
                { 
                    DiscordID = parameters[0], 
                    IGN = parameters[1] 
                });
            }
            catch
            {
                await ReplyAsync("Error processing add request: ID was invalid.");
            }

            await database.SaveChangesAsync();

            IUser user = await UserFromID(parameters[0]);

            await ReplyAsync($"Successfully added {user.Username} ({parameters[0]}) to the database as {parameters[1]}.");
        }

        [Command("removeentry")]
        [Summary("Removes a database entry; usage is !removeentry [id].")]
        public async Task RemoveEntry([Remainder] string id)
        {
            List<AccountData> toRemove = (await database.AccountData.ToListAsync()).Where(a => a.DiscordID == id).ToList();

            foreach (AccountData data in toRemove)
            {
                database.Remove(data);
            }

            await database.SaveChangesAsync();

            IUser user = await UserFromID(id);

            await ReplyAsync($"Successfully removed {user.Username} ({id}) from the database.");
        }

        [Command("listentries")]
        [Summary("Lists all database entries; usage is !listentries.")]
        public async Task ListEntries()
        {
            Embed embed = await CreateDatabaseEntriesEmbed();

            await ReplyAsync(embed: embed);
        }

        [Command("rank")]
        [Summary("Ranks all players in the database by a specific factor; usage is !rank [factor].")]
        public async Task RankAsync([Remainder] string factor)
        {
            List<PlayerData> playerDataEntries = (await database.AccountData.ToListAsync())
                .Select(async accountData => Utils.GetPlayerData(await Utils.GetPlayerToken(accountData.IGN), accountData.IGN))
                .Select(data => data.Result)
                .ToList();

            RankingFactor primaryRankingFactor = RankingFactor.None;

            foreach (RankingFactor rankingFactor in Utils.GetValues<RankingFactor>())
            {
                if (factor.IsPermutationOf(rankingFactor.ToString()))
                {
                    primaryRankingFactor = rankingFactor;
                }
            }

            if (primaryRankingFactor == RankingFactor.None)
            {
                return;
            }

            string fieldName = primaryRankingFactor.ToVariableName();

            FieldInfo fieldToSortBy = typeof(PlayerData).GetField(fieldName);

            Utils.SortPlayerDataListByFieldInfo(playerDataEntries, fieldToSortBy);

            Embed embed = await CreateLeaderboardEmbed(playerDataEntries, primaryRankingFactor, fieldToSortBy);

            await ReplyAsync(embed: embed);
        }

        [Command("bw")]
        [Summary("Returns the bedwars stats corresponding to a given IGN; usage is !bw [IGN].")]
        public async Task StatsAsync([Remainder] string ign)
        {
            JToken playerToken = await Utils.GetPlayerToken(ign);

            PlayerData playerData = Utils.GetPlayerData(playerToken, ign);

            int bedwarsGamesPlayed = Utils.GetBedwarsGamesPlayed(playerToken);

            await ReplyAsync(embed: CreateStatsEmbed(playerData, bedwarsGamesPlayed));
        }

        private Embed CreateStatsEmbed(PlayerData playerData, int gamesPlayed) 
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();

            string ign = IsSweaty(playerData, gamesPlayed) ? $"**{playerData.ign}**" : playerData.ign;

            embedBuilder
                .WithTitle($"Bedwars stats for {ign}:")
                .WithColor(ColorFromRank(playerData.rank))
                .AddField("Network Level:", playerData.networkLevel, true)
                .AddField("Rank:", playerData.rank, true)
                .AddField("Bedwars Stars:", playerData.bedwarsStars, true)
                .AddField("Wins:", playerData.wins, true)
                .AddField("Win/Loss Ratio:", $"{playerData.winLossRatio:N2}", true)
                .AddField("Beds Broken:", playerData.bedsBroken, true)
                .AddField("Final Kills:", playerData.finalKills, true)
                .AddField("Final Kill/Death Ratio:", $"{playerData.finalKD:N2}", true)
                .AddField("Winstreak:", playerData.winstreak, true);

            return embedBuilder.Build();
        }

        private async Task<Embed> CreateDatabaseEntriesEmbed()
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();

            List<AccountData> answers = await database.AccountData.ToListAsync();

            if (answers.Count == 0)
            {
                embedBuilder.WithTitle("No entries found.");

                return embedBuilder.Build();
            }
            else
            {
                embedBuilder.WithTitle("Current database entries:");
            }

            foreach (var entry in answers)
            {
                IUser user = await UserFromID(entry.DiscordID);

                embedBuilder.AddField($"{user.Username}:", $"{entry.IGN}\n", true);
            }

            return embedBuilder.Build();
        }

        private async Task<Embed> CreateLeaderboardEmbed(List<PlayerData> sortedList, RankingFactor rankingFactor, FieldInfo field)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();

            embedBuilder
                .WithTitle($"Bedwars rankings for {Context.Guild.Name} (by {rankingFactor.ToString().AddSpacesBetweenCapitals()}):")
                .WithColor(ColorFromRank(sortedList[0].rank));

            List<AccountData> accountData = await database.AccountData.ToListAsync();

            for (int i = 0; i < sortedList.Count; i++)
            {
                PlayerData playerData = sortedList[i];

                string id = await Utils.DiscordIDFromIGN(accountData, playerData.ign);

                IUser user = await UserFromID(id);

                object stat = field.GetValue(playerData);

                string value = stat is double ? $"{stat:N2}" : $"{stat}";

                embedBuilder.AddField($"#{i + 1}: {playerData.ign} ({user.Username}):", $"{value}");
            }

            return embedBuilder.Build();
        }

        private bool IsValidId(string id) => UserFromID(id) != null;

        private async Task<IUser> UserFromID(string id) => await Context.Client.GetUserAsync(ulong.Parse(id));

        private Color ColorFromRank(string rank) => rank switch 
        { 
            "None" => Color.DarkGrey,
            "VIP" => VipRank,
            "VIP+" => VipRank,
            "MVP" => MvpRank,
            "MVP+" => MvpRank,
            "MVP++" => MvpPlusPlusRank,
            _ => Color.Red
        };

        private bool IsSweaty(PlayerData playerData, int gamesPlayed) 
            => playerData.finalKD > MinimumFinalKD || playerData.winstreak > MinimumWinstreak
            || playerData.bedwarsStars > MinimumStars || playerData.winLossRatio > MinimumWinLossRatio 
            || gamesPlayed > MinimumGamesPlayed;
    }
}
