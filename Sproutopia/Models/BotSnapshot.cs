using Domain.Enums;
using Sproutopia.Enums;

namespace Sproutopia.Models
{
    public class BotSnapshot
    {
        public int Index { get; set; }
        public Guid BotId { get; set; }
        public string BotName { get; set; }
        public BotAction Action { get; set; }
        public BotAction Momentum { get; set; }
        public PowerUpType PowerUp { get; set; }
        public SuperPowerUpType SuperPowerUp { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public BotSnapshot(
            int index,
            Guid botId,
            string botName,
            BotAction action,
            BotAction momentum,
            PowerUpType powerUp,
            SuperPowerUpType superPowerUp,
            int x,
            int y)
        {
            Index = index;
            BotId = botId;
            BotName = botName;
            Action = action;
            Momentum = momentum;
            PowerUp = powerUp;
            SuperPowerUp = superPowerUp;
            X = x;
            Y = y;
        }
    }
}