using Discord;
using System;

namespace HypixelStatsBot
{
    public static partial class Constants
    {
        public static readonly string DiscordAPIKey = Environment.GetEnvironmentVariable("DISCORD_BOT_KEY");

        public static readonly string HypixelAPIKey = Environment.GetEnvironmentVariable("HYPIXEL_API_KEY");

        public const char Prefix = '!';

        public static readonly Color VipRank = new Color(85, 255, 85);

        public static readonly Color MvpRank = new Color(85, 255, 255);

        public static readonly Color MvpPlusPlusRank = new Color(255, 170, 0);
    }
}
