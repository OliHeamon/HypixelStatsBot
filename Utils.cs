using HypixelStatsBot.Database;
using HypixelStatsBot.Module;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static HypixelStatsBot.Constant.Constants;

namespace HypixelStatsBot
{
    public static class Utils
    {
        public static PlayerData GetPlayerData(JToken playerToken, string ign)
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

        public static async Task<JToken> GetPlayerToken(string ign)
        {
            string link = @$"https://api.hypixel.net/player?name={ign}&key={HypixelAPIKey}";

            using HttpClient client = new HttpClient();

            string response = await client.GetStringAsync(link);

            JObject jObject = JObject.Parse(response);

            return jObject.SelectToken("player");
        }

        private static int GetNetworkLevel(JToken playerToken)
        {
            double exp = playerToken.SelectToken("networkExp").Value<double>();

            return exp < 0 ? 1 : (int)Math.Floor(1 + ReversePqPrefix + Math.Sqrt(ReverseConst + GrowthDivides2 * exp));
        }

        private static string GetRank(JToken playerToken)
        {
            if (playerToken.SelectToken("monthlyPackageRank") != null
                && playerToken.SelectToken("monthlyPackageRank").Value<string>() == "SUPERSTAR")
            {
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

        private static int GetBedwarsStars(JToken playerToken) => playerToken.SelectToken("achievements").SelectToken("bedwars_level").Value<int>();

        private static int GetWins(JToken bedwarsStatsToken) => bedwarsStatsToken.SelectToken("wins_bedwars").Value<int>();

        private static double GetWinLossRatio(JToken bedwarsStatsToken)
        {
            double wins = bedwarsStatsToken.SelectToken("wins_bedwars").Value<double>();
            double losses = bedwarsStatsToken.SelectToken("losses_bedwars").Value<double>();

            return wins / losses;
        }

        private static int GetBedsBroken(JToken bedwarsStatsToken) => bedwarsStatsToken.SelectToken("beds_broken_bedwars").Value<int>();

        private static int GetFinalKills(JToken bedwarsStatsToken) => bedwarsStatsToken.SelectToken("final_kills_bedwars").Value<int>();

        private static double GetFinalKD(JToken bedwarsStatsToken)
        {
            double finalKills = bedwarsStatsToken.SelectToken("final_kills_bedwars").Value<double>();
            double finalDeaths = bedwarsStatsToken.SelectToken("final_deaths_bedwars").Value<double>();

            return finalKills / finalDeaths;
        }

        private static int GetWinStreak(JToken bedwarsStatsToken) => bedwarsStatsToken.SelectToken("winstreak").Value<int>();

        public static int GetBedwarsGamesPlayed(JToken playerToken) 
            => playerToken.SelectToken("stats").SelectToken("Bedwars").SelectToken("games_played_bedwars").Value<int>();

        // Tests if testString is a permutation of targetString (e.g. final kills is a permutation of Final Kills)
        public static bool IsPermutationOf(this string testString, string targetString) 
            => testString.Replace(" ", "").Trim().ToLower() == targetString.Replace(" ", "").Trim().ToLower();

        public static T[] GetValues<T>() where T : Enum => (T[])Enum.GetValues(typeof(T));

        public static string ToVariableName(this RankingFactor rankingFactor)
        {
            string name = rankingFactor.ToString();

            string firstCharacter = name.Substring(0, 1);

            string restOfName = name.Substring(1, name.Length - 1);

            return firstCharacter.ToLower() + restOfName;
        }

        public static void SortPlayerDataListByFieldInfo(List<PlayerData> playerData, FieldInfo field) 
            => playerData.Sort((entry1, entry2) => (field.GetValue(entry2) as IComparable).CompareTo(field.GetValue(entry1) as IComparable));

        public static string AddSpacesBetweenCapitals(this string text, bool preserveAcronyms = true)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            StringBuilder newText = new StringBuilder(text.Length * 2);

            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                {
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) 
                        || (preserveAcronyms && char.IsUpper(text[i - 1]) 
                        && i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                    {
                        newText.Append(' ');
                    }
                }

                newText.Append(text[i]);
            }

            return newText.ToString();
        }

        public static string DiscordIDFromIGN(List<AccountData> accountData, string ign)
            => accountData.FirstOrDefault(data => data.IGN == ign).DiscordID;
    }
}
