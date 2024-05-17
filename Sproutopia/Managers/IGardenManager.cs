using Domain.Enums;
using Sproutopia.Domain;
using Sproutopia.Enums;
using Sproutopia.Models;

namespace Sproutopia.Managers
{
    public interface IGardenManager
    {
        /// <summary>
        /// Issues an action for a bot to the GardenManager
        /// </summary>
        /// <param name="botId">ID of bot performing action</param>
        /// <param name="action">Action being performed</param>
        /// <returns>BotResponse</returns>
        public BotResponse PerformAction(Guid botId, BotAction action);

        /// <summary>
        /// Adds a new garden for a bot
        /// </summary>
        /// <param name="botId">ID of bot for which garden is to be added</param>
        public void AddBot(Guid botId, string nickname, string connectionId);

        /// <summary>
        /// Returns the number of powerups in the game world
        /// </summary>
        /// <returns>int</returns>
        public int PowerUpCount();

        /// <summary>
        /// Adds a new powerup to the game world
        /// </summary>
        public void AddPowerUp();

        /// <summary>
        /// Returns all powerups on the board by their position
        /// </summary>
        /// <returns>Dictionary<CellCoordinate, PowerUpType></returns>
        public Dictionary<CellCoordinate, PowerUpType> GetPowerUpTypes();

        /// <summary>
        /// Returns all super powerups on the board by their position
        /// </summary>
        /// <returns>Dictionary<CellCoordinate, SuperPowerUpType></returns>
        public Dictionary<CellCoordinate, SuperPowerUpType> GetSuperPowerUpTypes();

        /// <summary>
        /// Removes a specified powerup or super powerup from the game world
        /// </summary>
        /// <typeparam name="T">Type of powerup (PowerUpType / SuperPowerUpType)</typeparam>
        /// <param name="powerUpId">Id of powerup or super powerup to remove</param>
        public void RemovePowerUp<T>(Guid powerUpId) where T : Enum;

        /// <summary>
        /// Returns the number of weeds in the game world
        /// </summary>
        /// <returns>int</returns>
        public int WeedCount();

        /// <summary>
        /// Adds a new weed to the game world
        /// </summary>
        /// <param name="growthRate">Number of ticks to lapse before growing weed</param>
        public void AddWeed(int growthRate);

        /// <summary>
        /// Increments the size of an existing weed
        /// </summary>
        /// <param name="weedId">Id of weed to grow</param>
        /// <returns>Number of active weeds on the field</returns>
        public int GrowWeed(Guid weedId);


        /// <summary>
        /// Returns all active weeds
        /// </summary>
        /// <returns>IEnumerable<Weed></returns>
        public IEnumerable<Weed> GetWeeds();

        /// <summary>
        /// Respawns a bot in its starting location with starting garden
        /// </summary>
        /// <param name="botId">ID of bot to respawn</param>
        /// <returns>BotResponse</returns>
        public BotResponse RespawnBot(Guid botId);

        /// <summary>
        /// Returns current leaderboard
        /// </summary>
        /// <returns>IEnumerable<(Guid botId, int claimedPercentage)></returns>
        public IEnumerable<(Guid botId, int claimedPercentage)> Leaderboard();

        /// <summary>
        /// Returns a list of coordinates included in garden
        /// </summary>
        /// <param name="gardenId">Id of garden</param>
        /// <returns>IEnumerable<CellCoordinate></returns>
        public IEnumerable<CellCoordinate> GetGardenCellsById(Guid gardenId);

        /// <summary>
        /// Returns a list of coordinates included in trail
        /// </summary>
        /// <param name="gardenId">Id of trail</param>
        /// <returns>IEnumerable<CellCoordinate></returns>
        public IEnumerable<CellCoordinate> GetTrailCellsById(Guid gardenId);

        /// <summary>
        /// Serialise the map as a jagged byte array.
        /// </summary>
        /// <returns>A jagged array of CellType values representing the entire world</returns>
        public CellType[][] ViewGardens();

        /// <summary>
        /// Serialise the map within a given square window, centred on the given coordinates.
        /// </summary>
        /// <param name="x">The x coordinate of the centre of the window.</param>
        /// <param name="y">The y coordinate of the centre of the window.</param>
        /// <param name="size">
        /// The size of the area of the world map to be serialised. This window has to be symmetrical around the centre
        /// point so uneven numbers will have 1 added to them.
        /// </param>
        /// <returns>A jagged array of CellType values representing a section of the world.</returns>
        public CellType[][] ViewGardens(int x, int y, int size);

        /// <summary>
        /// Serialise the map within a given rectangular window, centred on the given coordinates.
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
        /// <returns>A jagged array of CellType values representing a section of the world.</returns>
        public CellType[][] ViewGardens(int x, int y, int width, int height);

        /// <summary>
        /// Serialise the weeds on the map as a jagged boolean area
        /// </summary>
        /// <returns>A jagged array of boolean values showing where weeds are present</returns>
        public bool[][] ViewWeeds();

        /// <summary>
        /// Serialise the weeds on the map within a given square window, centred on the given coordinates.
        /// </summary>
        /// <param name="x">The x coordinate of the centre of the window.</param>
        /// <param name="y">The y coordinate of the centre of the window.</param>
        /// <param name="size">
        /// The size of the area of the world map to be serialised. This window has to be symmetrical around the centre
        /// point so uneven numbers will have 1 added to them.
        /// </param>
        /// <returns>A jagged array of boolean values showing where weeds are present.</returns>
        public bool[][] ViewWeeds(int x, int y, int size);

        /// <summary>
        /// Serialise the weeds on the map within a given rectangular window, centred on the given coordinates.
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
        /// <returns>A jagged array of boolean values showing where weeds are present.</returns>
        public bool[][] ViewWeeds(int x, int y, int width, int height);

        /// <summary>
        /// Lists all powerups / super powerups on the map
        /// </summary>
        /// <returns>IEnumerable of (CellCoordinate, PowerUpType/SuperPowerUpType) tuples</returns>
        public IEnumerable<(CellCoordinate Coords, T PowerupType)> ViewPowerUps<T>() where T : Enum;

        /// <summary>
        /// Lists all powerups / super powerups on the map within a given square window, centred on the given coordinates.
        /// </summary>
        /// <param name="x">The x coordinate of the centre of the window.</param>
        /// <param name="y">The y coordinate of the centre of the window.</param>
        /// <param name="size">
        /// The size of the area of the world map to be serialised. This window has to be symmetrical around the centre
        /// point so uneven numbers will have 1 added to them.
        /// </param>
        /// <returns>IEnumerable of (CellCoordinate, PowerUpType/SuperPowerUpType) tuples</returns>
        public IEnumerable<(CellCoordinate Coords, T PowerupType)> ViewPowerUps<T>(int x, int y, int size) where T : Enum;

        /// <summary>
        /// List all powerups / super powerups on the map within a given rectangular window, centred on the given coordinates.
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
        /// <returns>IEnumerable of (CellCoordinate, PowerUpType/SuperPowerUpType) tuples</returns>
        public IEnumerable<(CellCoordinate Coords, T PowerupType)> ViewPowerUps<T>(int x, int y, int width, int height) where T : Enum;

        public string ToString();
    }
}
