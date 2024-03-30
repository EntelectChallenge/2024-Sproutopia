using Domain;
using Domain.Models;
using Sproutopia.Enums;
using Sproutopia.Models;

namespace Sproutopia.Managers
{
    public interface IBotManager
    {
        public int BotCount();

        public bool IsBotRegistered(Guid botId);

        public Dictionary<Guid, BotState> GetAllBotStates();

        public BotState GetBotState(Guid botId);

        public void SetBotState(BotState botState);

        public void SetBotPosition(Guid botId, CellCoordinate position, BotAction momentum);

        public void SetPowerUp(Guid botId, PowerUpType? powerUpType);

        public void SetSuperPowerUp(Guid botId, SuperPowerUpType? superPowerUpType);

        public void RespawnBot(Guid botId);

        public void AddBot(BotState botState);

        public Task<bool> EnqueueCommand(SproutBotCommand sproutBotCommand);

        public void ClearQueue(Guid botId);

        /// <summary>
        /// Lists all bots on the map
        /// </summary>
        /// <returns>IEnumerable of CellCoordinate</returns>
        public IEnumerable<CellCoordinate> ViewBots();

        /// <summary>
        /// Lists all bots on the map within a given square window, centred on the given coordinates.
        /// </summary>
        /// <param name="x">The x coordinate of the centre of the window.</param>
        /// <param name="y">The y coordinate of the centre of the window.</param>
        /// <param name="size">
        /// The size of the area of the world map to be serialised. This window has to be symmetrical around the centre
        /// point so uneven numbers will have 1 added to them.
        /// </param>
        /// <returns>IEnumerable of CellCoordinate</returns>
        public IEnumerable<CellCoordinate> ViewBots(int x, int y, int size);

        /// <summary>
        /// List all bots on the map within a given rectangular window, centred on the given coordinates.
        /// </summary>
        /// <param name="x">The x coordinate of the centre of the window.</param>
        /// <param name="y">The y coordinate of the centre of the window.</param>
        /// <param name="width">
        /// The horizontal size of the area of the world map to be serialised. This window has to be symmetrical around
        /// the centre point so uneven numbers will have 1 added to them.
        /// </param>
        /// <param name="height">
        /// The vertical size of the area of the world map to be serialised. This window has to be symmetrical around the
        /// centre point so uneven numbers will have 1 added to them.
        /// </param>
        /// <returns>IEnumerable of CellCoordinate</returns>
        public IEnumerable<CellCoordinate> ViewBots(int x, int y, int width, int height);
    }
}
