namespace NETCoreBot.Models
{
        public class Location
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class PowerUpLocation
    {
        public Location Location { get; set; }
        public int Type { get; set; }
    }

    public class BotStateDTO
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string ConnectionId { get; set; }
        public string ElapsedTime { get; set; }
        public int GameTick { get; set; }
        public List<List<int>> HeroWindow { get; set; }
        public int DirectionState { get; set; }
        public Dictionary<string, int> LeaderBoard { get; set; }
        public List<Location> BotPositions { get; set; }
        public List<PowerUpLocation> PowerUpLocations { get; set; }
        public List<List<bool>> Weeds { get; set; }
        public int PowerUp { get; set; }
        public int SuperPowerUp { get; set; }

        public string PrintWindow()
        {
            if (HeroWindow == null)
            {
                return "";
            }

            var window = "";
            for (int y = HeroWindow[0].Length - 1; y >= 0; y--)
            {
                for (int x = 0; x < HeroWindow.Length; x++)
                {
                    window += $"{HeroWindow[x][y]}";
                }
                window += "\n";
            }
            return window;
        }
    }
}
