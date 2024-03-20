namespace Domain.Models
{
    public class CloudCallback
    {
        public required string MatchId { get; set; }
        public required string MatchStatus { get; set; }
        public required string MatchStatusReason { get; set; }
        public string? Seed {  get; set; }
        public string? Ticks { get; set; }
        public List<CloudPlayer>? Players { get; set; }
    }
}
