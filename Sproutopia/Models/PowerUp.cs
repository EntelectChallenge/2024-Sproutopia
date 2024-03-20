using Sproutopia.Enums;

namespace Sproutopia.Models
{
    public class PowerUp
    {
        public Guid Id;
        public PowerUpType PowerUpType { get; private set; }
        public CellCoordinate Position { get; private set; }


        public PowerUp(PowerUpType powerUpType, CellCoordinate position)
        {
            Id = Guid.NewGuid();
            PowerUpType = powerUpType;
            Position = position;
        }
    }
}
