namespace Runner.DTOs
{
    /// <summary>
    /// Data given to the bots
    /// </summary>
    public class BotStateDTO
    {
        public int DirectionState { get; set; }
        public string ElapsedTime { get; set; }
        public int GameTick { get; set; }
        public int PowerUp { get; set; }
        public int SuperPowerUp { get; set; }
        /// <summary>
        /// Represents the current leader board with {Nickname, Territory percentage}   
        /// </summary>
        public Dictionary<Guid, int> LeaderBoard { get; set; }
        public Location[] BotPostions { get; set; }
        public PowerUpLocation[] PowerUpLocations { get; set; }
        public bool[][] Weeds { get; set; }
        public int[][] HeroWindow { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public BotStateDTO(
            int directionState,
            string elapsedTime,
            int gameTick,
            int powerUp,
            int superPowerUp,
            Dictionary<Guid, int> leaderBoard,
            Location[] botPostions,
            PowerUpLocation[] powerUpLocations,
            bool[][] weeds,
            int[][] heroWindow,
            int x,
            int y
        )
        {
            DirectionState = directionState;
            ElapsedTime = elapsedTime;
            GameTick = gameTick;
            PowerUp = powerUp;
            SuperPowerUp = superPowerUp;
            LeaderBoard = leaderBoard;
            BotPostions = botPostions;
            PowerUpLocations = powerUpLocations;
            Weeds = weeds;
            HeroWindow = heroWindow;
            X = x;
            Y = y;
        }
    }
    public struct PowerUpLocation
    {
        public Location Location { get; set; }
        public int Type { get; set; }
        public PowerUpLocation(Location location, int type)
        {
            Location = location;
            Type = type;
        }
    }

    public struct Location
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Location(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
