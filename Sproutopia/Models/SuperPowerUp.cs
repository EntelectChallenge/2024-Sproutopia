using Sproutopia.Enums;

namespace Sproutopia.Models
{
    public class SuperPowerUp
    {
        public Guid Id;
        public SuperPowerUpType PowerUpType { get; private set; }
        public CellCoordinate Position { get; private set; }


        public SuperPowerUp(SuperPowerUpType powerUpType, CellCoordinate position)
        {
            Id = Guid.NewGuid();
            PowerUpType = powerUpType;
            Position = position;
        }
    }
}
