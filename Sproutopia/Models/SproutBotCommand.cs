namespace Domain.Models
{
    public class SproutBotCommand : BotCommand, IComparable<SproutBotCommand>
    {
        public SproutBotCommand(Guid botId, BotAction action)
        {
            BotId = botId;
            Action = action;
            TimeStamp = DateTime.UtcNow;
        }

        public DateTime TimeStamp { get; set; }
        public BotAction Action { get; set; }

        public int CompareTo(SproutBotCommand? other)
        {
            return TimeStamp.CompareTo(other?.TimeStamp);
        }
    }

}
