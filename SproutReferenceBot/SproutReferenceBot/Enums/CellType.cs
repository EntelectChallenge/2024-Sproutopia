namespace SproutReferenceBot.Enums;

public enum CellType : byte
{
    Bot0Territory = 0,
    Bot1Territory = 1,
    Bot2Territory = 2,
    Bot3Territory = 3,
    Bot0Trail = 4,
    Bot1Trail = 5,
    Bot2Trail = 6,
    Bot3Trail = 7,
    OutOfBounds = 254,
    Unclaimed = 255,
}