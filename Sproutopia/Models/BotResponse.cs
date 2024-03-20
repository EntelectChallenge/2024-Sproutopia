using Sproutopia.Enums;

namespace Sproutopia.Models
{
    public class BotResponse
    {
        public CellCoordinate? NewPosition { get; set; }
        public bool Alive { get; set; } = true;
        public PowerUpType? PowerUpExcavated { get; set; }
        public SuperPowerUpType? SuperPowerUpExcavated { get; set; }
        public List<Guid> WeedsCleared { get; set; } = [];
        public List<Guid> BotsPruned { get; set; } = [];
        public List<Guid> BotsInterrupted { get; set; } = [];
    }
}
