using Domain.Enums;
using Domain.Models;
using Microsoft.Extensions.Options;
using Runner.DTOs;
using Sproutopia.Domain;
using Sproutopia.Enums;
using Sproutopia.Managers;

namespace Sproutopia.Models
{
    public class GameState
    {
        private SproutopiaGameSettings _gameSettings;

        public int MaxTicks { set; get; }
        public int CurrentTick { get; set; }
        public List<BotSnapshot> BotSnapshots { get; set; } = [];
        public int NumRows { get; private set; }
        public int NumCols { get; private set; }
        public int Seed { get; private set; }
        public int NumberOfPlayers { get; private set; }
        public int TickRate { get; private set; }
        public int PlayerWindowSize { get; private set; }
        public CellType[][] Land { get; set; }
        public IBotManager BotManager;
        public IGardenManager GardenManager;
        public int HeroWindowSize;

        public ChangeLog ChangeLogs { get; set; }
        public bool GameOver
        {
            get
            {
                return
                    CurrentTick >= MaxTicks ||
                    GardenManager.Leaderboard().MaxBy(b => b.claimedPercentage).claimedPercentage == 100;
            }
        }

        public GameState(IOptions<SproutopiaGameSettings> gameSettings, IBotManager botManager, IGardenManager gardenManager)
        {
            BotManager = botManager;
            GardenManager = gardenManager;

            _gameSettings = gameSettings.Value;

            MaxTicks = _gameSettings.MaxTicks;
            CurrentTick = 0;
            NumRows = _gameSettings.Rows;
            NumCols = _gameSettings.Cols;
            Seed = _gameSettings.Seed;
            NumberOfPlayers = _gameSettings.NumberOfPlayers;
            TickRate = _gameSettings.TickRate;
            PlayerWindowSize = _gameSettings.PlayerWindowSize;
            HeroWindowSize = _gameSettings.PlayerWindowSize;
        }

        public Guid AddBot(Guid botId, string nickname, string connectionId)
        {
            GardenManager.AddBot(botId, nickname, connectionId);

            return botId;
        }

        public (List<Guid> pruned, List<Guid> interrupted) IssueCommand(SproutBotCommand command)
        {
            var botResponse = GardenManager.PerformAction(command.BotId, command.Action);

            if (botResponse.NewPosition != null)
            {
                BotManager.SetBotPosition(command.BotId, botResponse.NewPosition, botResponse.Momentum);
            }

            if (botResponse.PowerUpExcavated != null)
            {
                BotManager.SetPowerUp(command.BotId, botResponse.PowerUpExcavated.PowerUpType);
                GardenManager.RemovePowerUp<PowerUpType>(botResponse.PowerUpExcavated.Id);
            }

            if (botResponse.SuperPowerUpExcavated != null)
            {
                BotManager.SetSuperPowerUp(command.BotId, botResponse.SuperPowerUpExcavated.PowerUpType);
                GardenManager.RemovePowerUp<SuperPowerUpType>(botResponse.SuperPowerUpExcavated.Id);
            }

            if (botResponse.AreaClaimed != 0)
            {
                BotManager.AwardTBP(command.BotId, botResponse.AreaClaimed);
            }

            return (botResponse.BotsPruned, botResponse.BotsInterrupted);
        }

        public void RespawnBot(Guid botId)
        {
            BotManager.RespawnBot(botId);
            GardenManager.RespawnBot(botId);
        }
        public void UpdateState(int currentTick)
        {
            CurrentTick = currentTick;
            Land = GardenManager.ViewGardens();
        }

        public bool AddWeed()
        {
            if (GardenManager.WeedCount() >= _gameSettings.WeedsMaxAmount)
                return false;

            GardenManager.AddWeed(_gameSettings.WeedGrowthRate);

            return true;
        }

        public bool GrowWeeds()
        {
            foreach (var weed in GardenManager.GetWeeds())
            {
                if (--weed.GrowthCountdown <= 0 && weed.Size < _gameSettings.WeedsMaxGrowth)
                {
                    GardenManager.GrowWeed(weed.Id);
                    weed.GrowthCountdown = _gameSettings.WeedGrowthRate;
                }
            }

            return true;
        }

        public bool AddPowerUp()
        {
            if (GardenManager.PowerUpCount() >= _gameSettings.PowerUpsMaxAmount)
                return false;

            GardenManager.AddPowerUp();

            return true;
        }

        public DiffLog MapToDiffLog(GameState previous)
        {
            var prevDiffLog = MapToDiffLog(new DiffLog());

            return MapToDiffLog(prevDiffLog);
        }

        public DiffLog MapToDiffLog(DiffLog previous)
        {
            var botIds = BotManager.BotIds();
            var botStates = BotManager.GetAllBotStates();

            int currentTick = CurrentTick;
            Dictionary<Guid, int> leaderBoard = GardenManager.Leaderboard().ToDictionary(tuple => tuple.botId, tuple => tuple.claimedPercentage);
            Dictionary<Guid, CellCoordinate> botPositions = botStates.Values.ToDictionary(b => b.BotId, b => b.Position);
            Dictionary<Guid, BotAction> botDirections = botStates.Values.ToDictionary(b => b.BotId, b => b.Momentum);
            Dictionary<Guid, PowerUpType> botPowerUps = botStates
                .Where(b => b.Value.GetActivePowerUp().HasValue)
                .ToDictionary(b => b.Key, b => b.Value.GetActivePowerUp()!.Value);
            Dictionary<Guid, SuperPowerUpType> botSuperPowerUps = botStates
                .Where(b => b.Value.GetActiveSuperPowerUp().HasValue)
                .ToDictionary(b => b.Key, b => b.Value.GetActiveSuperPowerUp()!.Value);
            Dictionary<CellCoordinate, int> territory = [];
            Dictionary<CellCoordinate, int> trails = [];
            foreach (var bot in botIds)
            {
                GardenManager.GetGardenCellsById(bot.Value).ToList().ForEach(c => territory[c] = bot.Key);
                GardenManager.GetTrailCellsById(bot.Value).ToList().ForEach(c => trails[c] = bot.Key);
            }
            Dictionary<CellCoordinate, bool> weeds = GardenManager.ViewWeeds()
                .SelectMany((row, rowIndex) => row.Select((value, colIndex) => new { RowIndex = rowIndex, ColIndex = colIndex, Value = value }))
                .Where(cell => cell.Value).ToDictionary(c => new CellCoordinate(c.ColIndex, c.RowIndex), c => true);
            Dictionary<CellCoordinate, PowerUpType> powerUps = GardenManager.ViewPowerUps<PowerUpType>().ToDictionary(p => p.Coords, p => p.PowerupType);
            Dictionary<CellCoordinate, SuperPowerUpType> superPowerUps = GardenManager.ViewPowerUps<SuperPowerUpType>().ToDictionary(p => p.Coords, p => p.PowerupType);

            var current = new DiffLog(
                currentTick: currentTick,
                botSnapshots: BotSnapshots,
                leaderBoard: leaderBoard,
                botPositions: botPositions,
                botDirections: botDirections,
                botPowerUps: botPowerUps,
                botSuperPowerUps: botSuperPowerUps,
                territory: territory,
                trails: trails,
                weeds: weeds,
                powerUps: powerUps,
                superPowerUps: superPowerUps
                );

            return new DiffLog(previous, current);
        }

        public GameStateDto MapAllToDto()
        {
            var heroWindow = GardenManager.ViewGardens().Select(rows =>
                rows.Select(cols => (int)cols).ToArray()).ToArray();

            var powerUps = GardenManager.ViewPowerUps<PowerUpType>().Select(pu => new PowerUpLocation(new(pu.Coords.X, pu.Coords.Y), (int)pu.PowerupType)).ToArray();
            var weeds = GardenManager.ViewWeeds();

            return new(CurrentTick, BotSnapshots, heroWindow, MapAllBotsToDto(), powerUps, weeds);
        }

        public GameInfoDTO MapToGameInfoDto()
        {
            var bots = BotManager.BotIds();

            var dto = new GameInfoDTO(
                maxTicks: MaxTicks,
                tickRate: TickRate,
                rows: NumRows,
                cols: NumCols,
                randomSeed: Seed,
                playerWindowSize: PlayerWindowSize,
                bots: bots
            );

            return dto;
        }

        public Dictionary<Guid, BotStateDTO> MapAllBotsToDto() =>
            BotManager.GetAllBotStates().ToDictionary(bot => bot.Key, bot => MapToBotDto(bot.Key));

        public BotStateDTO MapToBotDto(Guid botId)
        {
            var bot = BotManager.GetBotState(botId);
            var x = bot.Position.X;
            var y = bot.Position.Y;

            var heroWindow = GardenManager.ViewGardens(x, y, HeroWindowSize)
                .Select(rows => rows.Select(cols => (int)cols).ToArray())
                .ToArray();

            var opposingBotPosistions = BotManager.ViewBots(x, y, HeroWindowSize).Select(ob => new Location(ob.X, ob.Y)).ToArray();

            var powerUps = GardenManager.ViewPowerUps<PowerUpType>(x, y, HeroWindowSize)
                .Select(p => (p.Coords.X, p.Coords.Y, (int)p.PowerupType))
                .Union(
                    GardenManager.ViewPowerUps<SuperPowerUpType>(x, y, HeroWindowSize)
                    .Select(p => (p.Coords.X, p.Coords.Y, 10 + (int)p.PowerupType)))
                .ToList();

            var powerUpLocations = powerUps.Select(p => new PowerUpLocation(new(p.X, p.Y), p.Item3)).ToArray();

            var weeds = GardenManager.ViewWeeds(x, y, HeroWindowSize);

            return new BotStateDTO(
                directionState: (int)bot.Momentum,
                elapsedTime: TimeProvider.System.GetUtcNow().ToString(),
                gameTick: CurrentTick,
                powerUp: (int)(bot.GetActivePowerUp() ?? 0),
                superPowerUp: (int)(bot.GetActiveSuperPowerUp() ?? 0),
                leaderBoard: GardenManager.Leaderboard().ToDictionary(),
                botPostions: opposingBotPosistions,
                powerUpLocations: powerUpLocations,
                weeds: weeds,
                heroWindow: heroWindow,
                x: x,
                y: y);
        }
    }




}