using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using static HypixelStatsBot.Constants;

namespace HypixelStatsBot
{
    public class BedwarsModule : ModuleBase<SocketCommandContext> 
    {
        [Command("bw")]
        [Summary("Returns the bedwars stats corresponding to a given IGN; usage is !bw [IGN]")]
        public async Task StatsAsync([Remainder] string ign)
        {
            string link = @$"https://api.hypixel.net/player?name={ign}&key={HypixelAPIKey}";

            using HttpClient client = new HttpClient();

            string response = await client.GetStringAsync(link);

            JObject jObject = JObject.Parse(response);
            JToken playerToken = jObject.SelectToken("player");

            await ReplyAsync(embed: CreateFancyEmbed(GetPlayerData(playerToken, ign), GetGamesPlayed(playerToken.SelectToken("stats").SelectToken("Bedwars"))));
        }

        private Embed CreateFancyEmbed(PlayerData playerData, int gamesPlayed) 
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
                .AddField("Win/Loss Ratio:", playerData.winLossRatio, true)
                .AddField("Beds Broken:", playerData.bedsBroken, true)
                .AddField("Final Kills:", playerData.finalKills, true)
                .AddField("Final Kill/Death Ratio:", playerData.finalKD, true)
                .AddField("Winstreak:", playerData.winstreak, true);

            return embedBuilder.Build();
        }

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

        private PlayerData GetPlayerData(JToken playerToken, string ign)
        {
            JToken bedwarsStatsToken = playerToken.SelectToken("stats").SelectToken("Bedwars");

            PlayerData playerData = new PlayerData()
            {
                ign = ign,
                networkLevel = GetNetworkLevel(playerToken),
                rank = GetRank(playerToken),

                bedwarsStars = GetBedwarsStars(playerToken),

                wins = GetWins(bedwarsStatsToken),
                winLossRatio = GetWinLossRatio(bedwarsStatsToken),

                bedsBroken = GetBedsBroken(bedwarsStatsToken),

                finalKills = GetFinalKills(bedwarsStatsToken),
                finalKD = GetFinalKD(bedwarsStatsToken),

                winstreak = GetWinStreak(bedwarsStatsToken)
            };

            return playerData;
        }

        private string GetNetworkLevel(JToken playerToken)
        {
            double exp = playerToken.SelectToken("networkExp").Value<double>();

            int level = exp < 0 ? 1 : (int)Math.Floor(1 + ReversePqPrefix + Math.Sqrt(ReverseConst + GrowthDivides2 * exp));

            return $"{level}";
        }

        private string GetRank(JToken playerToken)
        {
            if (playerToken.SelectToken("monthlyPackageRank") != null
                && playerToken.SelectToken("monthlyPackageRank").Value<string>() == "SUPERSTAR") {
                return "MVP++";
            }

            if (playerToken.SelectToken("newPackageRank") == null)
            {
                return "None";
            }

            string rank = playerToken.SelectToken("newPackageRank").Value<string>();

            return rank switch
            {
                "NORMAL" => "None",
                "NONE" => "None",
                "VIP" => rank,
                "VIP_PLUS" => rank.Replace("_PLUS", "+"),
                "MVP" => rank,
                "MVP_PLUS" => rank.Replace("_PLUS", "+"),
                _ => "Other"
            };
        }

        private string GetBedwarsStars(JToken playerToken) => playerToken.SelectToken("achievements").SelectToken("bedwars_level").Value<string>();

        private string GetWins(JToken bedwarsStatsToken) => bedwarsStatsToken.SelectToken("wins_bedwars").Value<string>();

        private string GetWinLossRatio(JToken bedwarsStatsToken)
        {
            double wins = bedwarsStatsToken.SelectToken("wins_bedwars").Value<double>();
            double losses = bedwarsStatsToken.SelectToken("losses_bedwars").Value<double>();

            return $"{wins / losses:N2}";
        }

        private string GetBedsBroken(JToken bedwarsStatsToken) => bedwarsStatsToken.SelectToken("beds_broken_bedwars").Value<string>();

        private string GetFinalKills(JToken bedwarsStatsToken) => bedwarsStatsToken.SelectToken("final_kills_bedwars").Value<string>();

        private string GetFinalKD(JToken bedwarsStatsToken)
        {
            double finalKills = bedwarsStatsToken.SelectToken("final_kills_bedwars").Value<double>();
            double finalDeaths = bedwarsStatsToken.SelectToken("final_deaths_bedwars").Value<double>();

            return $"{finalKills / finalDeaths:N2}";
        }

        private string GetWinStreak(JToken bedwarsStatsToken) => bedwarsStatsToken.SelectToken("winstreak").Value<string>();

        private int GetGamesPlayed(JToken bedwarsStatsToken) => bedwarsStatsToken.SelectToken("games_played_bedwars").Value<int>();

        private bool IsSweaty(PlayerData playerData, int gamesPlayed) 
            => double.Parse(playerData.finalKD) > MinimumFinalKD || int.Parse(playerData.winstreak) > MinimumWinstreak
            || int.Parse(playerData.bedwarsStars) > MinimumStars || double.Parse(playerData.winLossRatio) > MinimumWinLossRatio 
            || gamesPlayed > MinimumGamesPlayed;
    }
}
