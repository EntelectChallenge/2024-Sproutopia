using Domain.Enums;
using Domain.Models;
using Microsoft.Extensions.Options;
using Sproutopia.Enums;
using Sproutopia.Models;

namespace Sproutopia.Managers
{
    public class BotManager : IBotManager
    {
        private readonly SproutopiaGameSettings _gameSettings;
        private readonly Dictionary<Guid, BotState> _bots;

        public BotManager(IOptions<SproutopiaGameSettings> gameSettings)
        {
            _gameSettings = gameSettings.Value;
            _bots = [];
        }

        public int BotCount()
        {
            return _bots.Count;
        }

        public bool IsBotRegistered(Guid botId) => _bots.ContainsKey(botId);

        public Dictionary<Guid, BotState> GetAllBotStates()
        {
            return _bots;
        }

        public BotState GetBotState(Guid botId)
        {
            return _bots[botId];
        }

        public void SetBotState(BotState botState)
        {
            if (!_bots.ContainsKey(botState.BotId))
            {
                throw new ArgumentException("Unknown bot", nameof(botState));
            }

            _bots[botState.BotId] = botState;
        }

        public void SetBotPosition(Guid botId, CellCoordinate position, BotAction momentum)
        {
            if (!_bots.ContainsKey(botId))
            {
                throw new ArgumentException("Unknown bot", nameof(botId));
            }

            _bots[botId].SetPosition(position, momentum);
        }

        public void SetPowerUp(Guid botId, PowerUpType? powerUpType)
        {
            if (!_bots.ContainsKey(botId))
            {
                throw new ArgumentException("Unknown bot", nameof(botId));
            }

            _bots[botId].SetActive(
                powerUpType,
                powerUpType switch
                {
                    PowerUpType.TerritoryImmunity => _gameSettings.LifespanImmunity,
                    PowerUpType.Unprunable => _gameSettings.LifespanUnprunable,
                    PowerUpType.Freeze => _gameSettings.LifespanFreeze,
                    _ => int.MaxValue,
                });
        }

        public void SetSuperPowerUp(Guid botId, SuperPowerUpType? superPowerUpType)
        {
            if (!_bots.ContainsKey(botId))
            {
                throw new ArgumentException("Unknown bot", nameof(botId));
            }

            _bots[botId].SetActive(
                superPowerUpType,
                superPowerUpType switch
                {
                    SuperPowerUpType.SuperFertilizer => _gameSettings.LifespanFertilizer,
                    _ => int.MaxValue,
                });
        }

        public void AwardTBP(Guid botId, int areaClaimed)
        {
            if (!_bots.ContainsKey(botId))
            {
                throw new ArgumentException("Unknown bot", nameof(botId));
            }

            _bots[botId].TieBreakingPoints += (int)Math.Pow(areaClaimed / 10.0, 2.0);
        }

        public void RespawnBot(Guid botId)
        {
            if (!_bots.ContainsKey(botId))
            {
                throw new ArgumentException("Unknown bot", nameof(botId));
            }

            _bots[botId].SetPosition(_bots[botId].RespawnPosition, BotAction.IDLE);
            _bots[botId].ClearActivePowerUp();
            _bots[botId].ClearActiveSuperPowerUp();
            ClearQueue(botId);
        }

        public void AddBot(BotState botState)
        {
            if (_bots.Count >= 4)
                throw new InvalidOperationException("Maximum number of bots exceeded");

            _bots[botState.BotId] = botState;
        }

        public Task<bool> EnqueueCommand(SproutBotCommand sproutBotCommand)
        {
            return _bots[sproutBotCommand.BotId].EnqueueCommand(sproutBotCommand);
        }

        public void ClearQueue(Guid botId)
        {
            _bots[botId].ClearQueue();
        }

        public Dictionary<int, Guid> BotIds()
        {
            return _bots.Select((pair, index) => new { Index = index, Key = pair.Key }).ToDictionary(x => x.Index, x => x.Key);
        }

        public IEnumerable<CellCoordinate> ViewBots()
        {
            foreach (var b in _bots)
                yield return b.Value.Position;
        }

        public IEnumerable<CellCoordinate> ViewBots(int x, int y, int size)
        {
            return ViewBots(x, y, size, size);
        }

        public IEnumerable<CellCoordinate> ViewBots(int x, int y, int width, int height)
        {
            int xRadius = 1 + width / 2;
            int yRadius = 1 + height / 2;

            var left = x - xRadius + 1;
            var right = x + xRadius - 1;
            var top = y - yRadius + 1;
            var bottom = y + yRadius - 1;

            foreach (var b in _bots.Values.Where(b => b.Position.WithinBounds(left, top, right, bottom)))
                yield return b.Position;
        }
    }
}
