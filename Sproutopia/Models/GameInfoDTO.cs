namespace Sproutopia.Models
{
    public class GameInfoDTO(int maxTicks, int tickRate, int rows, int cols, int randomSeed, int playerWindowSize, Dictionary<int, Guid> bots)
    {
        public int MaxTicks { get; private set; } = maxTicks;
        public int TickRate { get; private set; } = tickRate;
        public int Rows { get; private set; } = rows;
        public int Cols { get; private set; } = cols;
        public int RandomSeed { get; private set; } = randomSeed;
        public int PlayerWindowSize { get; private set; } = playerWindowSize;
        public Dictionary<int, Guid> Bots { get; private set; } = bots;
    }
}