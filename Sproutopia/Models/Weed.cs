using NetTopologySuite.Geometries;
using Sproutopia.Enums;
using Sproutopia.Utilities;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SproutopiaTests")]

namespace Sproutopia.Models
{

    public class Weed
    {
        public Guid Id { get; private set; }
        public int GrowthCountdown {  get; set; }
        public Geometry OvergrownLand
        {
            get
            {
                Geometry g = new Polygon(null);
                foreach(var cell in CoveredCells)
                {
                    g = g.Union(cell.ToCellInPointCoordinateSystem());
                }

                return g;
            }
        }
        public CellCoordinate StartingPosition;
        public List<CellCoordinate> CoveredCells = [];
        public SuperPowerUpType CorePowerUp;
        public int Size => CoveredCells.Count;

        public Weed(Guid Id, CellCoordinate startingPosition, SuperPowerUpType corePowerUp, int growthCountdown)
        {
            this.Id = Id;
            StartingPosition = startingPosition;
            CoveredCells.Add(startingPosition);
            CorePowerUp = corePowerUp;
            GrowthCountdown = growthCountdown;
        }

        public void UpdateGrowthCountDown(int growthCountdown)
        {
            GrowthCountdown = growthCountdown;
        }
    }
}