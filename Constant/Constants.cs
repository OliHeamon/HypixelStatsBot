using Discord;
using System;

namespace HypixelStatsBot.Constant
{
    public static partial class Constants
    {
        public static readonly string DiscordAPIKey = "ODAyOTEyMjU3NDI3MjQzMDA4.YA2ITw.IiEEiKuc6rsRIj-zWEx5CTPj7vE";// Environment.GetEnvironmentVariable("DISCORD_BOT_KEY");

        public static readonly string HypixelAPIKey = "be62d681-7717-4760-9104-6c2d24da0305"; //Environment.GetEnvironmentVariable("HYPIXEL_API_KEY");

        public const char Prefix = '!';

        public static readonly Color VipRank = new Color(85, 255, 85);

        public static readonly Color MvpRank = new Color(85, 255, 255);

        public static readonly Color MvpPlusPlusRank = new Color(255, 170, 0);
    }
}
