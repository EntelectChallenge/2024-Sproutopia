namespace Domain.Models
{
    public class CloudPlayer
    {
        public required string PlayerParticipantId { get; set; }
        public required string GamePlayerId { get; set; }
        public long FinalScore { get; set; }
        public int Placement { get; set; }
        public required string Seed { get; set; }
        public int MatchPoints { get; set; }
    }
}
