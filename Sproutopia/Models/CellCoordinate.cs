﻿using Domain.Enums;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Sproutopia.Utilities;

namespace Sproutopia.Models
{
    [JsonConverter(typeof(CellCoordinateConverter))]
    public class CellCoordinate : ValueObject
    {
        public int X { get; init; }
        public int Y { get; init; }

        public CellCoordinate()
        {
        }

        public CellCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public CellCoordinate(Point point)
        {
            X = (int)point.X;
            Y = (int)point.Y;
        }

        public CellCoordinate(Coordinate coord)
        {
            X = (int)coord.X;
            Y = (int)coord.Y;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return X;
            yield return Y;
        }

        public void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }

        public Coordinate ToPointCoordinate(bool offsetForCentre)
        {
            return new Coordinate(X + (offsetForCentre ? 0.5 : 0), Y + (offsetForCentre ? 0.5 : 0));
        }

        public CellCoordinate Offset(BotAction action)
        {
            return action switch
            {
                BotAction.Up => new CellCoordinate(X, Y - 1),
                BotAction.Down => new CellCoordinate(X, Y + 1),
                BotAction.Left => new CellCoordinate(X - 1, Y),
                BotAction.Right => new CellCoordinate(X + 1, Y),
                _ => this,
            };
        }

        public CellCoordinate Constrain(int width, int height)
        {
            return new CellCoordinate(
                Math.Min(Math.Max(0, X), width-1),
                Math.Min(Math.Max(0, Y), height-1));
        }

        public bool WithinBounds(int width, int height)
        {
            return X >= 0 && Y >= 0 && X < width && Y < height;
        }

        public bool WithinBounds(CellCoordinate topLeft, CellCoordinate bottomRight)
        {
            return X >= topLeft.X && Y >= topLeft.Y && X <= bottomRight.X && Y <= bottomRight.Y;
        }

        public bool WithinBounds(int x1, int y1, int x2, int y2)
        {
            return X >= x1 && Y >= y1 && X <= x2 && Y <= y2;
        }

        public override string ToString()
        {
            return $"{X},{Y}";
        }

        public static implicit operator CellCoordinate(string str)
        {
            string[] parts = str.Split(',');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int x) || !int.TryParse(parts[1], out int y))
            {
                throw new ArgumentException("Invalid string format for CellCoordinate conversion.");
            }

            return new CellCoordinate(x, y);
        }
    }
}