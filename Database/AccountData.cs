using System.ComponentModel.DataAnnotations;

namespace HypixelStatsBot.Database
{
    public class AccountData
    {
        [Key]
        public int EntryID { get; set; }

        public string DiscordID { get; set; }

        public string IGN { get; set; }
    }
}
