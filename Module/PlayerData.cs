namespace HypixelStatsBot.Module
{
    public struct PlayerData
    {
        // General
        public string ign;
        public int networkLevel;
        public string rank;

        // Bedwars
        public int bedwarsStars;

        public int wins;
        public double winLossRatio;

        public int bedsBroken;

        public int finalKills;
        public double finalKD;

        public int winstreak;
    }
}
