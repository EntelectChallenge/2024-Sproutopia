using Runner.DTOs;

namespace Sproutopia.Models
{
    public interface IGameState
    {
        public int CurrentTick { get; }
        public int NumRows { get; }
        public int NumCols { get; }
        public int Seed { get; }
        public Dictionary<Guid, IBotState> Bots { get; set; }
        public void AddBot(IBotState bot)
        {
            Bots.Add(bot.BotId, bot);
        }
        public Dictionary<Guid, BotStateDTO> MapAllBotsToDto();
    }
}