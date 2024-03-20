using Sproutopia.Models;

namespace Domain.Models
{
    public struct GameComplete
    {
        public int TotalTicks { get; set; }
        public List<PlayerResult> Players { get; set; }
        public int WorldSeed { get; set; }
        public IBotState WinngingBot { get; set; }
    }
}
