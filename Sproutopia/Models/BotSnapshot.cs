using Domain.Enums;

namespace Sproutopia.Models
{
    public class BotSnapshot
    {

        public DateTime TimeStamp { get; set; }
        public Guid BotId {  get; set; }
        public BotAction Action { get; set; }
        public BotAction Momentum { get; set; }

        public BotSnapshot(DateTime timeStamp, Guid botId, BotAction action, BotAction momentum)
        {
            TimeStamp = DateTime.UtcNow;
            BotId = botId;
            Action = action;
            Momentum = momentum;
        }
    }
}