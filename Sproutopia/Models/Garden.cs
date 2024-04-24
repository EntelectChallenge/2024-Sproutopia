using NetTopologySuite.Geometries;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("SproutopiaTests")]

namespace Sproutopia.Models
{
    public class Garden
    {
        public Guid Id { get; private set; }
        public Geometry ClaimedLand;
        public LineString? Trail;

        public int TrailLength => Trail != null ? (int)Trail.Length : 0;

        public int ClaimedArea => (int)ClaimedLand.Area;

        public bool HasTrail => Trail != null;

        public Garden(Guid Id, Geometry claimedLand)
        {
            this.Id = Id;
            ClaimedLand = claimedLand;
            Trail = null;
        }

        private static LineString AppendCoordinateToLine(LineString line, CellCoordinate coord)
        {
            return new LineString(line.Coordinates.Concat(new[] { new Coordinate(coord.X, coord.Y) }).ToArray());
        }

        public bool AddToTrail(CellCoordinate cellCoord)
        {
            Coordinate coordinate = new(cellCoord.X, cellCoord.Y);
            if (Trail == null)
            {
                Trail = new(new[] { coordinate, coordinate });
                return true;
            }

            if (Trail.EndPoint.Coordinate.Equals(coordinate))
            {
                return true;
            }

            if (new Point(coordinate).CoveredBy(Trail))
            {
                return false;
            }
            Trail = AppendCoordinateToLine(Trail, cellCoord);
            return true;
        }

        public void ClearTrail()
        {
            Trail = null;
        }

        /// <summary>
        /// Check whether a specified cell falls within the claimed land of the garden
        /// </summary>
        /// <param name="x">The x coordinate of the cell.</param>
        /// <param name="y">The y coordinate of the cell.</param>
        /// <returns>boolean</returns>
        public bool IsCellInClaimedLand(CellCoordinate cellCoord)
        {
            var pointCoord = cellCoord.ToPointCoordinate(true);
            return ClaimedLand.Covers(new Point(pointCoord.X, pointCoord.Y));
        }

        /// <summary>
        /// Check whether a specified cell is on the trail of the garden
        /// </summary>
        /// <param name="x">The x coordinate of the cell.</param>
        /// <param name="y">The y coordinate of the cell.</param>
        /// <returns>boolean</returns>
        public bool IsPointOnTrail(CellCoordinate cellCoord)
        {
            if (Trail == null)
                return false;

            return Trail.Distance(new Point(cellCoord.X, cellCoord.Y)) == 0;
        }

        /// <summary>
        /// Clears all claimed land of the territory.
        /// </summary>
        public void PruneClaimedLand()
        {
            ClaimedLand = new Polygon(null);
            Trail = null;
        }

        public IEnumerable<CellCoordinate> ClaimedLandAsCellCoordinates()
        {
            var envelope = ClaimedLand.Envelope;
            var minX = (int)envelope.Coordinates.ToList().MinBy(c => c.X)!.X;
            var minY = (int)envelope.Coordinates.ToList().MinBy(c => c.Y)!.Y;
            var maxX = (int)envelope.Coordinates.ToList().MaxBy(c => c.X)!.X;
            var maxY = (int)envelope.Coordinates.ToList().MaxBy(c => c.Y)!.Y;

            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    if (IsCellInClaimedLand(new CellCoordinate(x, y)))
                        yield return new CellCoordinate(x, y);
                }
            }
        }

        public IEnumerable<CellCoordinate> TrailAsCellCoordinates()
        {
            if (Trail != null)
            {
                foreach (var c in Trail.Coordinates)
                {
                    yield return new CellCoordinate(c);
                }
            }
        }

        public override string ToString()
        {
            var envelope = ClaimedLand.Envelope;
            var minX = (int)envelope.Coordinates.ToList().MinBy(c => c.X)!.X;
            var minY = (int)envelope.Coordinates.ToList().MinBy(c => c.Y)!.Y;
            var maxX = (int)envelope.Coordinates.ToList().MaxBy(c => c.X)!.X;
            var maxY = (int)envelope.Coordinates.ToList().MaxBy(c => c.Y)!.Y;

            var sb = new StringBuilder();
            sb.AppendLine($"{new string(' ', minY.ToString().Length)} {minX}");
            for (var y = minY; y <= maxY; y++)
            {
                if (y == minY)
                    sb.Append($"{minY.ToString()} ");
                else
                    sb.Append($"{new string(' ', minY.ToString().Length)} ");

                for (var x = minX; x <= maxX; x++)
                {
                    sb.Append(IsCellInClaimedLand(new CellCoordinate(x, y)) ? "#" : ".");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}