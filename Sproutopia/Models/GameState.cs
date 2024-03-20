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

        public void AddBot(Guid botId, string nickname, string connectionId)
        {
            GardenManager.AddBot(botId, nickname, connectionId);
        }

        public (List<Guid> pruned, List<Guid> interrupted) IssueCommand(SproutBotCommand command)
        {
            var botResponse = GardenManager.PerformAction(command.BotId, command.Action);

            if (botResponse.NewPosition != null)
            {
                BotManager.SetBotPosition(command.BotId, botResponse.NewPosition);
            }

            if (botResponse.PowerUpExcavated != null)
            {
                BotManager.SetPowerUp(command.BotId, botResponse.PowerUpExcavated);
            }

            if (botResponse.SuperPowerUpExcavated != null)
            {
                BotManager.SetSuperPowerUp(command.BotId, botResponse.SuperPowerUpExcavated);
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

        public GameStateDto MapAllToDto()
        {
            var heroWindow = GardenManager.ViewGardens().Select(rows =>
                rows.Select(cols => (int)cols).ToArray()).ToArray();

            var powerUps = GardenManager.ViewPowerUps<PowerUpType>().Select(pu => new PowerUpLocation(new(pu.Coords.X, pu.Coords.Y), (int)pu.PowerupType)).ToArray();
            var weeds = GardenManager.ViewWeeds();

            return new(CurrentTick, heroWindow, MapAllBotsToDto(), powerUps, weeds);

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
                directionState: (int)bot.LastCommand.Action,
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