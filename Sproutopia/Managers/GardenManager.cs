using Domain.Enums;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using Serilog;
using Sproutopia.Domain;
using Sproutopia.Enums;
using Sproutopia.Models;
using Sproutopia.Utilities;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("SproutopiaTests")]

namespace Sproutopia.Managers;

public class GardenManager : IGardenManager
{
    private readonly GlobalSeededRandomizer _randomizer;
    private readonly SproutopiaGameSettings _gameSettings;
    private readonly int _width;
    private readonly int _height;
    private readonly Dictionary<Guid, Garden> _gardens = [];
    private readonly Dictionary<Guid, Weed> _weeds = [];
    private readonly Dictionary<Guid, PowerUp> _powerUps = [];
    private readonly Dictionary<Guid, SuperPowerUp> _superPowerUps = [];
    private readonly List<CellCoordinate> _startingPositions = [];
    private readonly IBotManager _botManager;

    public IEnumerable<(Guid botId, int claimedPercentage)> Leaderboard()
    {
        var totalArea = _width * _height;
        foreach (var (botId, garden) in _gardens)
        {
            yield return (botId, garden.ClaimedArea * 100 / totalArea);
        }
    }

    public IEnumerable<CellCoordinate> GetGardenCellsById(Guid gardenId)
    {
        if (!_gardens.TryGetValue(gardenId, out var garden))
            throw new ArgumentException("Garden not found", nameof(gardenId));

        return garden.ClaimedLandAsCellCoordinates();
    }

    public IEnumerable<CellCoordinate> GetTrailCellsById(Guid gardenId)
    {
        if (!_gardens.TryGetValue(gardenId, out var garden))
            throw new ArgumentException("Garden not found", nameof(gardenId));

        return garden.TrailAsCellCoordinates();
    }

    public GardenManager(IOptions<SproutopiaGameSettings> gameSettings, IBotManager botManager, GlobalSeededRandomizer randomizer)
        : this(gameSettings.Value.Cols, gameSettings.Value.Rows, botManager, randomizer)
    {
        _gameSettings = gameSettings.Value;
    }

    /// <summary>
    /// Constructor to create new GardenManager with four bots positioned in their default starting positions and with their starting gardens.
    /// </summary>
    /// <param name="width">Width of newly created world</param>
    /// <param name="height">Height of newly created world</param>
    /// <param name="botIds">Array of botIds. Must have size of exactly four.</param>
    /// <param name="botManager">BotManager where bot info can be obtained</param>
    /// <exception cref="ArgumentException"></exception>
    public GardenManager(int width, int height, Guid[] botIds, IBotManager botManager, GlobalSeededRandomizer randomizer)
    {
        _randomizer = randomizer;
        _width = width;
        _height = height;
        _botManager = botManager;

        // Hardcoded initial positions for each bot which is halfway along the border in each quadrant, clockwise from the corner.
        _startingPositions.Add(new CellCoordinate(_width / 4, 1));
        _startingPositions.Add(new CellCoordinate(_width - 2, _height / 4));
        _startingPositions.Add(new CellCoordinate(_width - _width / 4 - 1, _height - 2));
        _startingPositions.Add(new CellCoordinate(1, _height - _height / 4 - 1));

        InitialiseGarden(botIds[0], _startingPositions[0].X, _startingPositions[0].Y, 3);
        InitialiseGarden(botIds[1], _startingPositions[1].X, _startingPositions[1].Y, 3);
        InitialiseGarden(botIds[2], _startingPositions[2].X, _startingPositions[2].Y, 3);
        InitialiseGarden(botIds[3], _startingPositions[3].X, _startingPositions[3].Y, 3);

        _botManager.AddBot(new BotState(0, botIds[0], _startingPositions[0]));
        _botManager.AddBot(new BotState(1, botIds[1], _startingPositions[1]));
        _botManager.AddBot(new BotState(2, botIds[2], _startingPositions[2]));
        _botManager.AddBot(new BotState(3, botIds[3], _startingPositions[3]));
    }

    /// <summary>
    /// Constructor to create new GardenManager with no bots
    /// </summary>
    /// <param name="width">Width of newly created world</param>
    /// <param name="height">Height of newly created world</param>
    /// <param name="botManager">BotManager where bot info can be obtained</param>
    public GardenManager(int width, int height, IBotManager botManager, GlobalSeededRandomizer randomizer)
    {
        _randomizer = randomizer;
        _width = width;
        _height = height;
        _botManager = botManager;

        // Hardcoded initial positions for each bot which is halfway along the border in each quadrant, clockwise from the corner.

        _startingPositions.Add(new CellCoordinate(_width / 4, 1));
        _startingPositions.Add(new CellCoordinate(_width - 2, _height / 4));
        _startingPositions.Add(new CellCoordinate(_width - _width / 4 - 1, _height - 2));
        _startingPositions.Add(new CellCoordinate(1, _height - _height / 4 - 1));
    }

    public void AddBot(Guid botId, string nickname, string connectionId)
    {
        if (_gardens.Count >= 4)
            throw new InvalidOperationException("Maximum number of bots exceeded");

        var pos = _startingPositions[_gardens.Count];

        InitialiseGarden(botId, pos.X, pos.Y, 3);

        _botManager.AddBot(new BotState(_botManager.BotCount(), botId, nickname, connectionId, pos));
    }

    public int PowerUpCount()
    {
        return _powerUps.Count;
    }

    public void AddPowerUp()
    {
        var newPosition = FindNewPowerUpPosition();

        if (newPosition != null)
        {
            var powerUp = new PowerUp(Helpers.RandomEnumValue<PowerUpType>(_randomizer.random), newPosition);
            _powerUps[powerUp.Id] = powerUp;
        }
    }

    public void RemovePowerUp<T>(Guid powerUpId) where T : Enum
    {
        if (typeof(T) == typeof(PowerUpType))
        {
            _powerUps.Remove(powerUpId);
        }
        else if (typeof(T) == typeof(SuperPowerUpType))
        {
            _superPowerUps.Remove(powerUpId);
        }
        else
        {
            throw new ArgumentException("Unknown PowerUp type specified", nameof(T));
        }
    }

    public Dictionary<CellCoordinate, PowerUpType> GetPowerUpTypes()
    {
        return _powerUps.ToDictionary(p => p.Value.Position, p => p.Value.PowerUpType);
    }

    public Dictionary<CellCoordinate, SuperPowerUpType> GetSuperPowerUpTypes()
    {
        return _superPowerUps.ToDictionary(p => p.Value.Position, p => p.Value.PowerUpType);
    }

    public int WeedCount()
    {
        return _weeds.Count;
    }

    public void AddWeed(int growthRate)
    {
        var strugglingBotId = Leaderboard().OrderBy(kv => kv.claimedPercentage).First().botId;

        var newPosition = FindNewWeedPosition(_botManager.GetBotState(strugglingBotId).Position);

        if (newPosition == null)
            return;

        var newId = Guid.NewGuid();

        _weeds[newId] = new Weed(newId, newPosition, Helpers.RandomEnumValue<SuperPowerUpType>(_randomizer.random), growthRate);

        // Grow the weed 4 times to give it some initial size (do we want to add this value to the game config?)
        for (int i = 0; i < 4; i++)
        {
            GrowWeed(newId);
        }
    }

    public int GrowWeed(Guid weedId)
    {
        List<BotAction> growthDir = new List<BotAction> { BotAction.Up, BotAction.Down, BotAction.Left, BotAction.Right }.Shuffle(_randomizer.random);

        if (!_weeds.TryGetValue(weedId, out var weed))
            throw new ArgumentException("Weed not found", nameof(weedId));

        var growthPoints = weed.CoveredCells.Shuffle<CellCoordinate>(_randomizer.random);
        foreach (var growthPoint in growthPoints)
        {
            foreach (var dir in growthDir)
            {
                var testCell = growthPoint.Offset(dir);

                // weed can't touch the border (bots must be able to encircle it)
                if (testCell.X <= 0 || testCell.Y <= 0 || testCell.X >= _width - 1 || testCell.Y >= _height - 1)
                    continue;

                // weed can't grow over other weed or back into itself
                if (_weeds.Values.Any(w => w.CoveredCells.Contains(testCell)))
                    continue;

                // weed can't grow over the spawn point of any bots
                if (_startingPositions.Any(s => s.Equals(testCell)))
                    continue;

                weed.CoveredCells.Add(testCell);
                return weed.CoveredCells.Count;
            }
        }

        return weed.CoveredCells.Count;
    }

    public IEnumerable<Weed> GetWeeds()
    {
        return _weeds.Values;
    }

    private CellCoordinate? FindNewPowerUpPosition()
    {
        Geometry searchArea = new Polygon(new LinearRing(new[]
        {
            new Coordinate(0, 0),
            new Coordinate(_width, 0),
            new Coordinate(_width, _height),
            new Coordinate(0, _height),
            new Coordinate(0, 0),
        }));

        // Exclude all existing trails from search area
        foreach (var (_, garden) in _gardens)
        {
            if (garden.HasTrail)
                searchArea = searchArea.Difference(garden.Trail!.ToPointCoordinateSystem());
        }

        var envelope = searchArea.Envelope;
        List<CellCoordinate> candidates = [];
        for (int y = (int)envelope.Coordinates.Min(c => c.Y); y < (int)envelope.Coordinates.Max(c => c.Y) - 1; y++)
        {
            for (int x = (int)envelope.Coordinates.Min(c => c.X); x < (int)envelope.Coordinates.Max(c => c.X) - 1; x++)
            {
                var testCell = new CellCoordinate(x, y);

                // Exclude cells not in the search area
                if (!searchArea.Covers(testCell.ToCellInPointCoordinateSystem()))
                    continue;

                // Exclude any cells occupied by other powerups
                if (_powerUps.Values.Any(p => p.Position == testCell))
                    continue;
                if (_superPowerUps.Values.Any(p => p.Position == testCell))
                    continue;

                // Exclude any cells occupied by weeds
                if (_weeds.Values.Any(w => w.CoveredCells.Contains(testCell)))
                    continue;

                candidates.Add(testCell);
            }
        }

        if (candidates.Count == 0)
            return null;

        // return random cell from list of candidates
        return candidates[_randomizer.Next(candidates.Count)];
    }

    private CellCoordinate? FindNewWeedPosition(CellCoordinate proximity)
    {
        int left = Math.Max(proximity.X - _width / 4, 1);
        int top = Math.Max(proximity.Y - _height / 4, 1);
        int right = Math.Min(proximity.X + _width / 4, _width - 1);
        int bottom = Math.Min(proximity.Y + _height / 4, _height - 1);

        // Establish search area as a quadrant sized rectangle around the proximity point, clipped by a
        // rectangle offset one cell from the boundaries (weeds are not allowed to spawn against the boundary)
        Geometry searchArea = new Polygon(new LinearRing(new[]
        {
            new Coordinate(left, top),
            new Coordinate(right, top),
            new Coordinate(right, bottom),
            new Coordinate(left, bottom),
            new Coordinate(left, top),
        }));

        // Exclude all existing territories and trails from search area
        foreach (var (_, garden) in _gardens)
        {
            searchArea = searchArea.Difference(garden.ClaimedLand);
            if (garden.HasTrail)
                searchArea = searchArea.Difference(garden.Trail!.ToPointCoordinateSystem());
        }

        List<CellCoordinate> candidates = [];
        for (int y = top; y < bottom; y++)
        {
            for (int x = left; x < right; x++)
            {
                var testCell = new CellCoordinate(x, y);

                // Exclude cells not in the search area
                if (!searchArea.Covers(testCell.ToCellInPointCoordinateSystem()))
                    continue;

                // Exclude any cells occupied by bots
                if (_botManager.GetAllBotStates().Values.Any(b => b.Position.Equals(testCell)))
                    continue;

                // Exclude any cells occupied by other powerups
                if (_powerUps.Values.Any(p => p.Position == testCell))
                    continue;
                if (_superPowerUps.Values.Any(p => p.Position == testCell))
                    continue;

                // Exclude any cells occupied by other weeds
                if (_weeds.Values.Any(w => w.CoveredCells.Contains(testCell)))
                    continue;

                // Exclude any cells coinciding with spawn points of bots
                if (_startingPositions.Any(s => s.Equals(testCell)))
                    continue;

                candidates.Add(testCell);
            }
        }

        if (candidates.Count == 0)
            return null;

        // return random cell from list of candidates
        return candidates[_randomizer.Next(candidates.Count)];
    }

    public BotResponse PerformAction(Guid botId, BotAction action)
    {
        List<Guid> prunedBots = new();
        List<Guid> interruptedBots = new();
        var bot = _botManager.GetBotState(botId) ?? throw new ArgumentException("Unknown bot", nameof(botId));
        var botGarden = GetGardenById(botId) ?? throw new ApplicationException("GardenManager has no garden for bot");
        var currentPosition = _botManager.GetBotState(botId).Position;
        var newPosition = currentPosition.Offset(action);
        PowerUp? powerUpExcavated = null;
        SuperPowerUp? superPowerUpExcavated = null;
        List<Guid> weedsCleared = [];
        int areaClaimed = 0;

        // Decrement bot's powerup countdowns
        bot.PowerupsCountdown();

        // If any other bots have Freeze powerup active, don't move
        if (_botManager.GetAllBotStates().Any(b => b.Key != botId && b.Value.IsActive(PowerUpType.Freeze)))
        {
            return new BotResponse
            {
                NewPosition = currentPosition,
                Momentum = bot.Momentum
            };
        }

        // Will command take bot out of bounds?
        if (!newPosition.WithinBounds(_width, _height))
        {
            // Maintain momentum in stead
            action = bot.Momentum;
            newPosition = currentPosition.Offset(action);

            // Will momentum still take bot out of bounds?
            if (!newPosition.WithinBounds(_width, _height))
            {
                // Don't move bot
                return new BotResponse
                {
                    NewPosition = currentPosition,
                    Momentum = bot.Momentum
                };
            }
        }

        // Check if another bot's territory is being entered
        foreach (var (otherId, otherBot) in _botManager.GetAllBotStates().Where(b => b.Key != botId && GetGardenAtCell(newPosition)?.Id == b.Key))
        {
            // If current bot has Super Fertilizer active, it gets cancelled
            if (bot.IsActive(SuperPowerUpType.SuperFertilizer))
            {
                bot.ClearActiveSuperPowerUp();
            }

            // If entered territory belongs to another bot which has Territory Immunity active, freeze current bot
            if (otherId != botId && otherBot.IsActive(PowerUpType.TerritoryImmunity))
            {
                return new BotResponse
                {
                    NewPosition = currentPosition,
                    Momentum = bot.Momentum
                };
            }
        }

        // Check for collision with another bot
        foreach (var (otherId, otherBot) in _botManager.GetAllBotStates().Where(b => b.Key != botId && !prunedBots.Contains(b.Key)))
        {
            if (otherBot.Position == newPosition)
            {
                var otherGarden = GetGardenById(otherId);

                // Check if other bot has Unprunable powerup active
                if (otherBot.IsActive(PowerUpType.Unprunable))
                {
                    if (otherGarden.HasTrail)
                        otherBot.SetPosition(new CellCoordinate(otherGarden.Trail!.StartPoint), BotAction.IDLE);
                    else
                        otherBot.SetPosition(otherBot.RespawnPosition, BotAction.IDLE);

                    otherBot.ClearQueue();
                    otherGarden.ClearTrail();
                    interruptedBots.Add(otherId);
                }
                // Check if collision is happening inside other bot's territory
                else if (GetGardenAtCell(newPosition)?.Id == otherGarden.Id)
                {
                    prunedBots.Add(botId);
                    TransferGarden(botId, otherId);

                    return new BotResponse
                    {
                        NewPosition = newPosition,
                        Momentum = BotAction.IDLE,
                        Alive = false,
                        BotsPruned = prunedBots,
                        BotsInterrupted = interruptedBots,
                    };
                }
                else
                {
                    prunedBots.Add(otherId);
                    TransferGarden(otherId, botId);
                }
            }
        }

        // Check for collision with weed
        if (_weeds.Values.Any(w => w.CoveredCells.Contains(newPosition)))
        {
            prunedBots.Add(botId);
            PruneGarden(botId);

            return new BotResponse
            {
                NewPosition = newPosition,
                Momentum = BotAction.IDLE,
                Alive = false,
                BotsPruned = prunedBots,
                BotsInterrupted = interruptedBots,
            };
        }

        // Check if bot cuts off any trail
        var trailGarden = GetGardenTrail(newPosition);
        if (trailGarden != null && !prunedBots.Contains(trailGarden.Id))
        {
            if (trailGarden.Id != botId)
            {
                // Severed trail belongs to a different bot
                var otherBot = _botManager.GetBotState(trailGarden.Id);

                // Check if other bot has Unprunable powerup active
                if (otherBot.IsActive(PowerUpType.Unprunable))
                {
                    otherBot.SetPosition(new CellCoordinate(trailGarden.Trail!.StartPoint), BotAction.IDLE);
                    otherBot.ClearQueue();
                    trailGarden.ClearTrail();
                    interruptedBots.Add(trailGarden.Id);
                }
                else
                {
                    prunedBots.Add(trailGarden.Id);
                    TransferGarden(trailGarden.Id, botId);
                }
            }
            else
            {
                // Severed trail belongs to self

                // If bot is in IDLE state, it means it's recently respawned without momentum.
                // This does not constitute a collision with its own trail
                if (bot.Momentum != BotAction.IDLE)
                {
                    // Check if bot has Trail Protection active
                    if (bot.IsActive(SuperPowerUpType.TrailProtection))
                    {
                        bot.ClearActiveSuperPowerUp();
                        bot.ClearQueue();

                        var startPoint = new CellCoordinate(botGarden.Trail!.StartPoint);
                        trailGarden.ClearTrail();

                        return new BotResponse
                        {
                            NewPosition = startPoint,
                            Momentum = BotAction.IDLE,
                            Alive = true,
                            BotsPruned = prunedBots,
                            BotsInterrupted = interruptedBots,
                        };
                    }
                    else
                    {
                        prunedBots.Add(botId);
                        PruneGarden(botId);

                        return new BotResponse
                        {
                            NewPosition = newPosition,
                            Momentum = BotAction.IDLE,
                            Alive = false,
                            BotsPruned = prunedBots,
                            BotsInterrupted = interruptedBots,
                        };
                    }
                }
            }
        }

        // Check if powerup is excavated
        powerUpExcavated = _powerUps.Values.FirstOrDefault(p => p.Position == newPosition);
        if (powerUpExcavated != null)
        {
            Log.Debug($"Bot {bot.BotId} excavated {powerUpExcavated.PowerUpType}");
        }

        // Check if super powerup is excavated
        superPowerUpExcavated = _superPowerUps.Values.FirstOrDefault(p => p.Position == newPosition);
        if (superPowerUpExcavated != null)
        {
            Log.Debug($"Bot {bot.BotId} excavated {superPowerUpExcavated.PowerUpType}");
        }

        // If Super Fertilizer is active, expand territory
        if (bot.IsActive(SuperPowerUpType.SuperFertilizer))
        {
            // Construct new blob around bot's new position
            Geometry newBlob = GetFertilizerBlob(action, newPosition);

            // Subtract any weed geometries from blob
            foreach (var w in _weeds.Values)
            {
                newBlob = newBlob.Difference(w.OvergrownLand);
            }

            botGarden.ClaimedLand = botGarden.ClaimedLand.Union(newBlob);
        }
        else
        {
            if (!botGarden.ClaimedLand.Covers(new Point(newPosition.ToPointCoordinate(true))))
            {
                // If new bot position is outside of claimed land, add to trail

                // If no active trail exists, it means the bot has just left its claimed land and started laying down
                // a trail. In that case, add the current position to the trail before adding the new position so as
                // to ensure the trail starts just inside the claimed land as opposed to just outside.
                if (!botGarden.HasTrail)
                {
                    botGarden.AddToTrail(currentPosition);
                }

                botGarden.AddToTrail(newPosition);
            }
            else if (!botGarden.ClaimedLand.Covers(new Point(currentPosition.ToPointCoordinate(true))))
            {
                // If new bot position is inside of claimed land, but old position is outside, add new position to trail
                botGarden.AddToTrail(newPosition);
            }

            if (HasGardenCompletedTrail(botId))
            {
                var newClaimed = CompleteTrail(botId);
                List<Guid> encircledBots = new();

                // Check if newly claimed area covers any other bots and prune them if so
                if (newClaimed != null)
                {
                    areaClaimed = (int)newClaimed.Area;
                    foreach (var otherBot in _botManager.GetAllBotStates().Where(bp => bp.Key != botId))
                    {
                        if (newClaimed.Covers(new Point(otherBot.Value.Position.ToPointCoordinate(true))))
                        {
                            encircledBots.Add(otherBot.Key);
                            TransferGarden(otherBot.Key, botId);
                        }
                    }
                }

                prunedBots.AddRange(encircledBots.Except(prunedBots));

                // Check if new claimed territory entirely covers a weed
                foreach (var weed in _weeds.Values.Where(w => _gardens[botId].ClaimedLand.Covers(w.OvergrownLand)))
                {
                    weedsCleared.Add(weed.Id);
                    var newSuperPowerUp = new SuperPowerUp(weed.CorePowerUp, weed.StartingPosition);
                    _superPowerUps[newSuperPowerUp.Id] = newSuperPowerUp;

                    Log.Debug($"{newSuperPowerUp.PowerUpType} uncovered at {weed.StartingPosition}");
                }
                foreach (var weedId in weedsCleared)
                {
                    _weeds.Remove(weedId);
                }

                // Check if new claimed territory steals any territory from another bot
                foreach (var g in _gardens.Values.Where(g => g.Id != botId))
                {
                    g.ClaimedLand = g.ClaimedLand.Difference(_gardens[botId].ClaimedLand);
                }
            }

            // If super fertilizer has been excavated, convert trail to territory
            if (superPowerUpExcavated?.PowerUpType == SuperPowerUpType.SuperFertilizer && botGarden.HasTrail)
            {
                CompleteTrail(botId);
            }
        }

        return new BotResponse
        {
            NewPosition = newPosition,
            Momentum = action,
            Alive = true,
            AreaClaimed = areaClaimed,
            BotsPruned = prunedBots,
            BotsInterrupted = interruptedBots,
            PowerUpExcavated = powerUpExcavated,
            SuperPowerUpExcavated = superPowerUpExcavated,
            WeedsCleared = weedsCleared,
        };
    }

    private Geometry GetFertilizerBlob(BotAction action, CellCoordinate newPosition)
    {
        Geometry newBlob = newPosition.ToCellInPointCoordinateSystem();

        var posBehind = action switch
        {
            BotAction.Up => newPosition.Offset(BotAction.Down),
            BotAction.Down => newPosition.Offset(BotAction.Up),
            BotAction.Left => newPosition.Offset(BotAction.Right),
            BotAction.Right => newPosition.Offset(BotAction.Left),
            _ => newPosition,
        };
        var posBehindLeft = action switch
        {
            BotAction.Up => posBehind.Offset(BotAction.Left),
            BotAction.Down => posBehind.Offset(BotAction.Right),
            BotAction.Left => posBehind.Offset(BotAction.Down),
            BotAction.Right => posBehind.Offset(BotAction.Up),
            _ => newPosition,
        };
        var posBehindRight = action switch
        {
            BotAction.Up => posBehind.Offset(BotAction.Right),
            BotAction.Down => posBehind.Offset(BotAction.Left),
            BotAction.Left => posBehind.Offset(BotAction.Up),
            BotAction.Right => posBehind.Offset(BotAction.Down),
            _ => newPosition,
        };

        newBlob = newBlob.Union(posBehind.ToCellInPointCoordinateSystem());
        newBlob = newBlob.Union(posBehindLeft.Constrain(_width, _height).ToCellInPointCoordinateSystem());
        newBlob = newBlob.Union(posBehindRight.Constrain(_width, _height).ToCellInPointCoordinateSystem());

        return newBlob;
    }

    public BotResponse RespawnBot(Guid botId)
    {
        if (!_gardens.TryGetValue(botId, out var garden))
        {
            throw new ArgumentException("Unknown bot", nameof(botId));
        }

        // Ensure that respawned bot's garden is pruned
        PruneGarden(botId);

        // Create new starting garden for bot
        int size = 3;
        int radius = 1 + size / 2;

        var respawnPosition = _botManager.GetBotState(botId).RespawnPosition;

        int left = Math.Max(respawnPosition.X - radius + 1, 0);
        int top = Math.Max(respawnPosition.Y - radius + 1, 0);
        int right = Math.Min(respawnPosition.X + radius, _width);
        int bottom = Math.Min(respawnPosition.Y + radius, _height);

        var polygon = new Polygon(new LinearRing(new[]
        {
            new Coordinate(left, top),
            new Coordinate(right, top),
            new Coordinate(right, bottom),
            new Coordinate(left, bottom),
            new Coordinate(left, top),
        }));

        _gardens[botId] = new Garden(botId, polygon);

        return new BotResponse
        {
            NewPosition = respawnPosition,
            Momentum = BotAction.IDLE,
            Alive = true
        };
    }

    /// <summary>
    /// Initialise a new garden with specified size around specified initial position.
    /// 
    /// If a garden with specified id already exits, it is overwritten. If not, it is added to the list of garden.
    /// If the list of gardens is already full, an ArgumentException is thrown.
    /// </summary>
    /// <param name="id">Unique identifier for garden.</param>
    /// <param name="x">The x coordinate for the cell at the centre of the garden.</param>
    /// <param name="y">The y coordinate for the cell at the centre of the garden.</param>
    /// <param name="size">
    /// The size of the initial claimed area in the garden surrounding
    /// the specified coordinates. The initial claimed area must be symmetrical around the
    /// centre point. As such, uneven numbers will simply have 1 added to it.
    /// </param>
    /// <returns>The newly initialised garden.</returns>
    internal Garden InitialiseGarden(Guid Id, int x, int y, int size)
    {
        _gardens.TryGetValue(Id, out var garden);
        if (garden == null && _gardens.Count == 4)
            throw new InvalidOperationException("Maximum number of gardens exceeded");

        int radius = 1 + size / 2;

        int left = Math.Max(x - radius + 1, 0);
        int top = Math.Max(y - radius + 1, 0);
        int right = Math.Min(x + radius, _width);
        int bottom = Math.Min(y + radius, _height);

        var polygon = new Polygon(new LinearRing(new[]
        {
            new Coordinate(left, top),
            new Coordinate(right, top),
            new Coordinate(right, bottom),
            new Coordinate(left, bottom),
            new Coordinate(left, top),
        }));

        _gardens[Id] = new Garden(Id, polygon);

        return _gardens[Id];
    }

    /// <summary>
    /// Get a garden based on its Id
    /// </summary>
    /// <param name="botId">The bot id for which to find the garden.</param>
    /// <returns>The garden for the specified bot id</returns>
    internal Garden GetGardenById(Guid botId)
    {
        if (!_gardens.TryGetValue(botId, out var garden))
            throw new ArgumentException("Garden not found", nameof(botId));

        return garden;
    }

    /// <summary>
    /// Get the territory that the given cell belongs to.
    /// </summary>
    /// <param name="coord">The cell coordinate</param>
    /// <returns>The territory that owns the given cell, or null if it is unclaimed.</returns>
    internal Garden? GetGardenAtCell(CellCoordinate coord)
    {
        var garden =
            _gardens.Values.FirstOrDefault(elem => elem.ClaimedLand.Covers(new Point(coord.ToPointCoordinate(true))));

        return garden;
    }

    /// <summary>
    /// Get a garden that entirely encloses the given garden.
    /// </summary>
    /// <param name="botId">The bot's garden to check.</param>
    /// <returns>The garden that entirely encloses this bot's garden, or null if there is none.</returns>
    internal Garden? GetEnclosingGarden(Guid botId)
    {
        if (!_gardens.TryGetValue(botId, out var garden))
            throw new ArgumentException("Bot not found", nameof(botId));

        var otherBot =
            _gardens.Values.FirstOrDefault(elem => elem != garden && elem.ClaimedLand.Covers(garden.ClaimedLand));

        return otherBot;
    }

    /// <summary>
    /// Remove all claimed land in the given garden.
    /// </summary>
    /// <param name="botId">The bot's garden to prune.</param>
    /// <returns>The resulting garden after pruning. Returns null if garden is not found.</returns>
    internal Garden? PruneGarden(Guid botId)
    {
        if (!_gardens.TryGetValue(botId, out var garden))
            throw new ArgumentException("Garden not found", nameof(botId));

        garden.PruneClaimedLand();

        return garden;
    }

    /// <summary>
    /// Has the given garden "completed" their trail.
    /// </summary>
    /// <param name="botId">The bot's garden to check.</param>
    /// <returns>True if the trail is completed, false if not.</returns>
    internal bool HasGardenCompletedTrail(Guid botId)
    {
        if (!_gardens.TryGetValue(botId, out var garden))
            throw new ArgumentException("Garden not found", nameof(botId));

        LineString? gardenTrail = garden.Trail;
        if (gardenTrail == null)
        {
            return false;
        }

        CellCoordinate trailEndCell = new((int)gardenTrail.EndPoint.X, (int)gardenTrail.EndPoint.Y);
        Geometry gardenLand = garden.ClaimedLand;

        return gardenLand.Covers(new Point(trailEndCell.ToPointCoordinate(true)));
    }

    /// <summary>
    /// Complete the trail, and add the land to the given garden.
    /// </summary>
    /// <param name="botId">The bot's garden to complete the loop for.</param>
    /// <returns>Newly claimed land</returns>
    internal Geometry? CompleteTrail(Guid botId)
    {
        Geometry? newClaimed = null;

        if (!_gardens.TryGetValue(botId, out var garden))
            throw new ArgumentException("Garden not found", nameof(botId));

        if (garden.Trail == null)
            throw new ArgumentException("Garden has no trail", nameof(botId));

        Geometry? territoryStart = null;
        Geometry? territoryEnd = null;

        for (int i = 0; i < garden.ClaimedLand.NumGeometries; i++)
        {
            var territory = garden.ClaimedLand.GetGeometryN(i);
            if (territory.Covers(garden.Trail.StartPoint))
            {
                territoryStart = territory;
            }
            if (territory.Covers(garden.Trail.EndPoint))
            {
                territoryEnd = territory;
            }

            if (territoryStart != null && territoryEnd != null)
                break;
        }

        if (territoryStart != territoryEnd)
        {
            // Union of start teritory, end territory and trail
            var trailGeo = garden.Trail.ToPointCoordinateSystem();
            var newPoly = (territoryStart ?? Polygon.Empty).Union(territoryEnd).Union(trailGeo) as Polygon;
            var exterior = new Polygon(new LinearRing(newPoly!.ExteriorRing.Coordinates));
            var oldClaimed = garden.ClaimedLand;
            garden.ClaimedLand = garden.ClaimedLand.Union(exterior);
            newClaimed = garden.ClaimedLand.Difference(oldClaimed);

            // Reset trail
            garden.Trail = null;
        }
        else
        {
            // Union of start/end teritory and trail
            var trailGeo = garden.Trail.ToPointCoordinateSystem();
            var newGeo = (territoryStart! as Polygon)!.Union(trailGeo);
            var exterior = new Polygon(new LinearRing((newGeo as Polygon)!.ExteriorRing.Coordinates));
            newClaimed = exterior.Difference(newGeo);

            garden.ClaimedLand = garden.ClaimedLand.Union(exterior);

            // Exclude starting territories of other bots from claimed land
            garden.ClaimedLand = _gardens.Values
                .Where(g => g.Id != garden.Id)
                .Aggregate(garden.ClaimedLand, (claimedLand, botGarden) => claimedLand.Difference(botGarden.HomeBase));

            // Reset trail
            garden.ClearTrail();
        }

        return newClaimed;
    }

    /// <summary>
    /// Transfer all claimed land from one bot to another.
    /// </summary>
    /// <param name="sourceBotId">The bot to transfer land from.</param>
    /// <param name="destinationBotId">The bot to transfer land to.</param>
    internal bool TransferGarden(Guid sourceBotId, Guid destinationBotId)
    {
        if (sourceBotId == destinationBotId)
            throw new ArgumentException("Identical source and destination");

        if (!_gardens.TryGetValue(sourceBotId, out var gardenSrc))
            throw new ArgumentException("Garden not found", nameof(sourceBotId));

        if (!_gardens.TryGetValue(destinationBotId, out var gardenDst))
            throw new ArgumentException("Garden not found", nameof(destinationBotId));

        //Take the difference of claimed land and startingPosition in order to avoid taking the starting land
        var landToClaim = gardenSrc.ClaimedLand.Difference(gardenSrc.HomeBase);

        gardenDst.ClaimedLand = gardenDst.ClaimedLand.Union(landToClaim);

        gardenSrc.PruneClaimedLand();

        return true;
    }

    /// <summary>
    /// Return the garden that has a trail at the given cell.
    /// </summary>
    /// <param name="cell">The cell to check</param>
    /// <returns>The garden that owns a trail at the given cell coordinate, or null if there is none.</returns>
    internal Garden? GetGardenTrail(CellCoordinate cell)
    {
        return _gardens.Values.FirstOrDefault(garden => garden.IsPointOnTrail(cell));
    }

    public CellType[][] ViewGardens()
    {
        var retval = Helpers.CreateJaggedArray<CellType[][]>(_width, _height);

        for (int cx = 0; cx < _width; cx++)
        {
            for (int cy = 0; cy < _height; cy++)
            {
                retval[cx][cy] = CellType.Unclaimed;

                var translatedCellCoord = new CellCoordinate(cx, cy);

                foreach (var item in _gardens.Values.Select((garden, i) => new { i, garden }))
                {
                    var garden = item.garden;
                    var index = item.i;

                    if (garden.IsCellInClaimedLand(translatedCellCoord))
                    {
                        retval[cx][cy] = (CellType)index;
                        break;
                    }
                }

                foreach (var item in _gardens.Values.Select((garden, i) => new { i, garden }))
                {
                    var garden = item.garden;
                    var index = item.i;

                    if (garden.IsPointOnTrail(translatedCellCoord) && retval[cx][cy] != (CellType)index)
                    {
                        retval[cx][cy] = (CellType)(index + 4);
                        break;
                    }
                }
            }
        }

        return retval;
    }

    public CellType[][] ViewTerritories()
    {
        var retval = Helpers.CreateJaggedArray<CellType[][]>(_width, _height);

        for (int cx = 0; cx < _width; cx++)
        {
            for (int cy = 0; cy < _height; cy++)
            {
                retval[cx][cy] = CellType.Unclaimed;

                var translatedCellCoord = new CellCoordinate(cx, cy);

                foreach (var item in _gardens.Values.Select((garden, i) => new { i, garden }))
                {
                    var garden = item.garden;
                    var index = item.i;

                    if (garden.IsCellInClaimedLand(translatedCellCoord))
                    {
                        retval[cx][cy] = (CellType)index;
                        break;
                    }
                }
            }
        }

        return retval;
    }

    public CellType[][] ViewTrails()
    {
        var retval = Helpers.CreateJaggedArray<CellType[][]>(_width, _height);

        for (int cx = 0; cx < _width; cx++)
        {
            for (int cy = 0; cy < _height; cy++)
            {
                retval[cx][cy] = CellType.Unclaimed;

                var translatedCellCoord = new CellCoordinate(cx, cy);

                foreach (var item in _gardens.Values.Select((garden, i) => new { i, garden }))
                {
                    var garden = item.garden;
                    var index = item.i;

                    if (garden.IsPointOnTrail(translatedCellCoord) && retval[cx][cy] != (CellType)index)
                    {
                        retval[cx][cy] = (CellType)(index + 4);
                        break;
                    }
                }
            }
        }

        return retval;
    }

    public CellType[][] ViewGardens(int x, int y, int size)
    {
        return ViewGardens(x, y, size, size);
    }

    public CellType[][] ViewGardens(int x, int y, int width, int height)
    {
        int xRadius = 1 + width / 2;
        int yRadius = 1 + height / 2;

        CellType[][] retval = new CellType[xRadius * 2 - 1][];

        for (int cx = 0; cx < xRadius * 2 - 1; cx++)
        {
            retval[cx] = new CellType[yRadius * 2 - 1];
            for (int cy = 0; cy < yRadius * 2 - 1; cy++)
            {
                var translatedX = x - xRadius + cx + 1;
                var translatedY = y - yRadius + cy + 1;

                retval[cx][cy] = CellType.Unclaimed;
                if (translatedX < 0 || translatedX >= _width || translatedY < 0 || translatedY >= _height)
                {
                    retval[cx][cy] = CellType.OutOfBounds;
                    continue;
                }

                var translatedCellCoord = new CellCoordinate(translatedX, translatedY);

                foreach (var item in _gardens.Values.Select((garden, i) => new { i, garden }))
                {
                    var garden = item.garden;
                    var index = item.i;

                    if (garden.IsCellInClaimedLand(translatedCellCoord))
                    {
                        retval[cx][cy] = (CellType)index;
                        break;
                    }
                }

                foreach (var item in _gardens.Values.Select((garden, i) => new { i, garden }))
                {
                    var garden = item.garden;
                    var index = item.i;

                    if (garden.IsPointOnTrail(translatedCellCoord))
                    {
                        retval[cx][cy] = (CellType)(index + 4);
                        break;
                    }
                }
            }
        }

        return retval;
    }

    public bool[][] ViewWeeds()
    {
        var retval = Helpers.CreateJaggedArray<bool[][]>(_width, _height);

        for (int cx = 0; cx < _width; cx++)
        {
            for (int cy = 0; cy < _height; cy++)
            {
                var testCell = new CellCoordinate(cx, cy);
                retval[cx][cy] = _weeds.Any(w => w.Value.CoveredCells.Contains(testCell));
            }
        }

        return retval;
    }

    public bool[][] ViewWeeds(int x, int y, int size)
    {
        return ViewWeeds(x, y, size, size);
    }

    public bool[][] ViewWeeds(int x, int y, int width, int height)
    {
        int xRadius = 1 + width / 2;
        int yRadius = 1 + height / 2;

        var retval = Helpers.CreateJaggedArray<bool[][]>(xRadius * 2 - 1, yRadius * 2 - 1);

        for (int cx = 0; cx < xRadius * 2 - 1; cx++)
        {
            for (int cy = 0; cy < yRadius * 2 - 1; cy++)
            {
                var translatedX = x - xRadius + cx + 1;
                var translatedY = y - yRadius + cy + 1;

                var testCell = new CellCoordinate(translatedX, translatedY);
                retval[cx][cy] = _weeds.Any(w => w.Value.CoveredCells.Contains(testCell));
            }
        }

        return retval;
    }

    public IEnumerable<(CellCoordinate Coords, T PowerupType)> ViewPowerUps<T>() where T : Enum
    {
        if (typeof(T) == typeof(PowerUpType))
        {
            foreach (var p in _powerUps)
                yield return (p.Value.Position, (T)(object)p.Value.PowerUpType);
        }

        if (typeof(T) == typeof(SuperPowerUpType))
        {
            foreach (var p in _superPowerUps)
                yield return (p.Value.Position, (T)(object)p.Value.PowerUpType);
        }
    }

    public IEnumerable<(CellCoordinate Coords, T PowerupType)> ViewPowerUps<T>(int x, int y, int size) where T : Enum
    {
        return ViewPowerUps<T>(x, y, size, size);
    }

    public IEnumerable<(CellCoordinate Coords, T PowerupType)> ViewPowerUps<T>(int x, int y, int width, int height) where T : Enum
    {
        int xRadius = 1 + width / 2;
        int yRadius = 1 + height / 2;

        var left = x - xRadius + 1;
        var right = x + xRadius - 1;
        var top = y - yRadius + 1;
        var bottom = y + yRadius - 1;

        if (typeof(T) == typeof(PowerUpType))
        {
            foreach (var p in _powerUps.Values.Where(p => p.Position.WithinBounds(left, top, right, bottom)))
                yield return (p.Position, (T)(object)p.PowerUpType);
        }

        if (typeof(T) == typeof(SuperPowerUpType))
        {
            foreach (var p in _superPowerUps.Values.Where(p => p.Position.WithinBounds(left, top, right, bottom)))
                yield return (p.Position, (T)(object)p.PowerUpType);
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new();

        var world = ViewGardens();
        for (int y = 0; y < _height; y++)
        {
            if (y != 0)
                sb.AppendLine();

            for (int x = 0; x < _width; x++)
            {
                sb.Append(world[x][y] switch
                {
                    CellType.Bot0Territory => "A",
                    CellType.Bot1Territory => "B",
                    CellType.Bot2Territory => "C",
                    CellType.Bot3Territory => "D",
                    CellType.Bot0Trail => "a",
                    CellType.Bot1Trail => "b",
                    CellType.Bot2Trail => "c",
                    CellType.Bot3Trail => "d",
                    CellType.OutOfBounds => "#",
                    _ => ".",
                });
            }
        }

        return sb.ToString();
    }
}