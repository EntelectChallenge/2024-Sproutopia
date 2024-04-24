using Domain;
using Sproutopia.Enums;

namespace Sproutopia.Models
{
    public class BotResponse
    {
        public CellCoordinate? NewPosition { get; set; }
        public BotAction Momentum { get; set; }
        public bool Alive { get; set; } = true;
        public int AreaClaimed { get; set;  } = 0;
        public PowerUp? PowerUpExcavated { get; set; }
        public SuperPowerUp? SuperPowerUpExcavated { get; set; }
        public List<Guid> WeedsCleared { get; set; } = [];
        public List<Guid> BotsPruned { get; set; } = [];
        public List<Guid> BotsInterrupted { get; set; } = [];
    }
}
