using Runner.DTOs;
using Sproutopia.Domain;

namespace Sproutopia.Models
{
    public class GameStateDto
    {
        public int CurrentTick { get; set; }
        /*   public int NumRows { get; }
             public int NumCols { get; }*/
        //   public int Seed { get; }
        public int[][] Land { get; set; }
        public Dictionary<Guid, BotStateDTO> Bots { get; set; }
        public Dictionary<CellType, CellCoordinate> ChangeLog { get; set; }
        public PowerUpLocation[] PowerUps { get; set; }

        public bool[][] Weeds { get; set; }
        public GameStateDto(int currentTick,
        //    int seed,
            int[][] land,
            Dictionary<Guid, BotStateDTO> bots,
             // List<(int x, int y, int type)> PowerUps,
             PowerUpLocation[] powerUps,
            bool[][] weeds)
        {
            CurrentTick = currentTick;
            /*   NumRows = numRows;
                 NumCols = numCols;*/
            //   Seed = seed;
            Land = land;
            Bots = bots;
            ChangeLog = [];
            Weeds = weeds;
            PowerUps = powerUps;
        }
    }
}