using NetTopologySuite.Geometries;
using Sproutopia.Domain;
using Sproutopia.Models;

namespace Sproutopia.Utilities
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Converts a coordinate in point coordinate system to a polygon in point coordinate system defining the cell with specified coordinate at top left corner
        /// </summary>
        /// <param name="coordinate">Point coordinate of top left corner</param>
        /// <returns>Polygon describing cell</returns>
        public static Geometry ToCellInPointCoordinateSystem(this Coordinate coordinate)
        {
            return new Polygon(new LinearRing(new Coordinate[]
            {
                new(coordinate.X, coordinate.Y),
                new(coordinate.X+1, coordinate.Y),
                new(coordinate.X+1, coordinate.Y+1),
                new(coordinate.X, coordinate.Y+1),
                new(coordinate.X, coordinate.Y),
            }));
        }

        /// <summary>
        /// Converts a coordinate in cell coordinate system to a polygon in point coordinate system defining the cell
        /// </summary>
        /// <param name="coordinate">Cell coordinate of cell to convert</param>
        /// <returns>Polygon describing cell</returns>
        public static Geometry ToCellInPointCoordinateSystem(this CellCoordinate coordinate)
        {
            return coordinate.ToPointCoordinate(false).ToCellInPointCoordinateSystem();
        }

        /// <summary>
        /// Converts a LineString to a polygon in point coordinate system
        /// </summary>
        /// <param name="lineString">LineString of point coordinates</param>
        /// <returns>Polygon of cells for point coordinates</returns>
        public static Geometry ToPointCoordinateSystem(this LineString lineString)
        {
            var response = lineString.StartPoint.Coordinate.ToCellInPointCoordinateSystem();

            for (int i = 0; i < lineString.NumPoints - 1; i++)
            {
                foreach (var coordinate in lineString.GetPointN(i).Coordinate.ConnectTo(lineString.GetPointN(i+1).Coordinate))
                {
                    response = response.Union(coordinate.ToCellInPointCoordinateSystem());
                }
            }

            return response;
        }

        /// <summary>
        /// Adds all unit coordinates to polygon
        /// </summary>
        /// <param name="polygon">Polygon with arbitrary number of coordinates, not necesarily unit length apart</param>
        /// <returns>Polygon with all coordinates exactly one unit apart</returns>
        public static Polygon Intrapolate(this Polygon polygon)
        {
            List<Coordinate> coordinates = new() { polygon.Coordinates.First().Copy() };
            for (int i = 1; i < polygon.Coordinates.Length; i++)
            {
                var last = coordinates.Last();
                foreach (var coordinate in last!.ConnectTo(polygon.Coordinates[i]).Skip(1))
                {
                    if (!coordinates.Contains(coordinate))
                        coordinates.Add(coordinate.Copy());
                }
            }

            if (coordinates.First() != coordinates.Last())
                coordinates.Add(coordinates.First().Copy());

            return new Polygon(new LinearRing(coordinates.ToArray()));
        }

        /// <summary>
        /// Returns coordinates that comprise LineString
        /// </summary>
        /// <param name="lineString"></param>
        /// <returns>IEnumerable of coordinates for LineString</returns>
        public static IEnumerable<Coordinate> ToCoordinates(this LineString lineString)
        {
            for (int i = 0; i < lineString.NumPoints; i++)
            {
                foreach (var coordinate in lineString.GetPointN(i).Coordinate.ConnectTo(lineString.GetPointN(i + 1).Coordinate))
                {
                    yield return coordinate;
                }
            }
        }


        /// <summary>
        /// Connects two coordinates with all intermediate coordinates of unit distance apart in between
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static IEnumerable<Coordinate> ConnectTo(this Coordinate start, Coordinate end)
        {
            var current = start.Copy();
            int dx = Math.Abs((int)end.X - (int)current.X);
            int dy = Math.Abs((int)end.Y - (int)current.Y);
            int sx = (int)current.X < (int)end.X ? 1 : -1;
            int sy = (int)current.Y < (int)end.Y ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                yield return new Coordinate { X = current.X, Y = current.Y };

                if (current.X == end.X && current.Y == end.Y)
                    yield break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    current.X += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    current.Y += sy;
                }
            }
        }

        /// <summary>
        /// Returns a shuffled version of an input List
        /// </summary>
        /// <typeparam name="T">Type of elements in List</typeparam>
        /// <param name="list">List to be shuffled</param>
        /// <param name="rng">Random number generator to use for the shuffle</param>
        /// <returns>Shuffled List<T></returns>
        public static List<T> Shuffle<T>(this List<T> list, Random rng)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
            return list;
        }
    }
}
