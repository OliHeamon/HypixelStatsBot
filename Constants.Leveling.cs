namespace HypixelStatsBot
{
    public static partial class Constants
    {
        private const double baseAmount = 10000;

        public const double ReversePqPrefix = -(baseAmount - 0.5 * 2500) / 2500;
        public const double ReverseConst = ReversePqPrefix * ReversePqPrefix;
        public const double GrowthDivides2 = 1 / 1250.0;
    }
}
