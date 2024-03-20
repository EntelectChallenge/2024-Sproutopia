using Sproutopia.Domain;
using Sproutopia.Models;

namespace Domain.Models
{
    public class ChangeLog
    {
        CellType[][] land;
        Dictionary<Guid, CellCoordinate> botPostions;
        Dictionary<Guid, CellCoordinate> weeds;
        private Dictionary<Guid, PowerUp> _powerUps = [];
        private Dictionary<Guid, SuperPowerUp> _superPowerUps = [];
    }
}
