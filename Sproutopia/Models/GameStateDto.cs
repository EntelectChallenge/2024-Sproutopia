using Domain.Enums;
using Domain.Models;
using Newtonsoft.Json;
using Runner.DTOs;
using Sproutopia.Domain;
using Sproutopia.Enums;
using Sproutopia.Utilities;
using System.Text;

namespace Sproutopia.Models
{
    public class GameStateDto
    {
        public int CurrentTick { get; set; }
        public List<BotSnapshot> BotSnapshots { get; set; }
        public int[][] Land { get; set; }
        public Dictionary<Guid, BotStateDTO> Bots { get; set; }
        public Dictionary<CellType, CellCoordinate> ChangeLog { get; set; }
        public PowerUpLocation[] PowerUps { get; set; }
        public bool[][] Weeds { get; set; }

        public GameStateDto(int currentTick,
            List<BotSnapshot> botSnapshots,
            int[][] land,
            Dictionary<Guid, BotStateDTO> bots,
            PowerUpLocation[] powerUps,
            bool[][] weeds)
        {
            CurrentTick = currentTick;
            BotSnapshots = botSnapshots;
            Land = land;
            Bots = bots;
            ChangeLog = [];
            Weeds = weeds;
            PowerUps = powerUps;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Tick: {CurrentTick}");
            sb.AppendLine();
            foreach (var (id, botState) in Bots)
            {
                var botSnapshot = BotSnapshots.FirstOrDefault(b => b.BotId == id);
                sb.AppendLine($"{id.ToString().Substring(0, 4)}...: {botState.X},{botState.Y}->{(BotAction)botState.DirectionState}({botSnapshot?.Action})");
            }
            sb.AppendLine();
            sb.AppendLine("Power Ups:");
            foreach (var (id, botState) in Bots)
            {
                sb.AppendLine($"{id.ToString().Substring(0, 4)}...: {(PowerUpType)botState.PowerUp}");
            }
            sb.AppendLine();
            sb.AppendLine("Super Power Ups:");
            foreach (var (id, botState) in Bots)
            {
                sb.AppendLine($"{id.ToString().Substring(0, 4)}...: {(SuperPowerUpType)botState.SuperPowerUp}");
            }

            return sb.ToString();
        }

        public GameStateDto DeepCopy()
        {
            // Using Json serialization/deserialization for deep copy as a quick and dirty alternative for the deep copy code I wrote below
            // as that had some bugs which I'm too lazy to figure out now.
            return JsonConvert.DeserializeObject<GameStateDto>(JsonConvert.SerializeObject(this));

            GameStateDto copy = new GameStateDto(
                currentTick : CurrentTick,
                botSnapshots : BotSnapshots, // I don't think a deep copy is necessary here as we're not doing a diff on this field but I'll have to confirm
                land : Helpers.DeepCopy2DArray(Land),
                bots : Helpers.DeepCopyDictionary(Bots),
                powerUps : Helpers.DeepCopyArray(PowerUps),
                weeds : Helpers.DeepCopy2DArray(Weeds)
            );
            return copy;
        }

        public GameStateDto ApplyDiffs(List<DiffLog> diffLogs)
        {
            if ((diffLogs?.Count ?? 0) == 0)
                throw new ArgumentException("No logs provided", nameof(diffLogs));

            var newGameState = DeepCopy();

            foreach (var diffLog in diffLogs!)
            {
                newGameState = newGameState.ApplyDiff(diffLog);
            }

            return newGameState;
        }

        public GameStateDto ApplyDiff(DiffLog diffLog)
        {
            var newGameState = DeepCopy();
            newGameState.CurrentTick = diffLog.CurrentTick;
            newGameState.BotSnapshots = diffLog.BotSnapshots;

            foreach (var bot in newGameState.Bots)
            {
                bot.Value.GameTick = diffLog.CurrentTick;
            }

            foreach (var (key, botPosition) in diffLog.BotPositions)
            {
                if (!newGameState.Bots.ContainsKey(key))
                    newGameState.Bots[key] = new BotStateDTO(
                        directionState: 0,
                        elapsedTime: "",
                        gameTick: diffLog.CurrentTick,
                        powerUp: 0,
                        superPowerUp: 0,
                        leaderBoard: [],
                        botPostions: [],
                        powerUpLocations: [],
                        weeds: null,
                        heroWindow: null,
                        x: botPosition.X,
                        y: botPosition.Y
                        );

                newGameState.Bots[key].X = botPosition.X;
                newGameState.Bots[key].Y = botPosition.Y;
            }

            foreach (var (key, botDir) in diffLog.BotDirections)
            {
                newGameState.Bots[key].DirectionState = (int)botDir;
            }

            foreach (var (key, botPowerUp) in diffLog.BotPowerUps)
            {
                newGameState.Bots[key].PowerUp = (int)botPowerUp;
            }

            foreach (var (key, botSuperPowerUp) in diffLog.BotSuperPowerUps)
            {
                newGameState.Bots[key].SuperPowerUp = (int)botSuperPowerUp;
            }

            foreach (var (coord, territory) in diffLog.Territory)
            {
                newGameState.Land[coord.X][coord.Y] = territory;
            }

            foreach (var (coord, trail) in diffLog.Trails)
            {
                if (newGameState.Land[coord.X][coord.Y] != trail)
                {
                    if (trail == 255)
                        newGameState.Land[coord.X][coord.Y] = newGameState.Land[coord.X][coord.Y] <= 3 ? newGameState.Land[coord.X][coord.Y] : trail;
                    else
                        newGameState.Land[coord.X][coord.Y] = trail + 4;
                }
            }

            foreach (var (coord, weed) in diffLog.Weeds)
            {
                newGameState.Weeds[coord.X][coord.Y] = weed;
            }

            foreach (var (coord, powerUp) in diffLog.PowerUps)
            {
                int indexToRemove = Array.FindIndex(newGameState.PowerUps, p => p.Location.X == coord.X && p.Location.Y == coord.Y);
                if (indexToRemove != -1)
                {
                    newGameState.PowerUps = newGameState.PowerUps.Where((num, index) => index != indexToRemove).ToArray();
                }

                if (powerUp != 0)
                {
                    var powerups = newGameState.PowerUps.ToList();
                    powerups.Add(new PowerUpLocation(new Location(coord.X, coord.Y), (int)powerUp));
                    newGameState.PowerUps = powerups.ToArray();
                }
            }

            foreach (var (coord, superPowerUp) in diffLog.SuperPowerUps)
            {
                int indexToRemove = Array.FindIndex(newGameState.PowerUps, p => p.Location.X == coord.X && p.Location.Y == coord.Y);
                if (indexToRemove != -1)
                {
                    newGameState.PowerUps = newGameState.PowerUps.Where((num, index) => index != indexToRemove).ToArray();
                }

                if (superPowerUp != 0)
                {
                    var pul = newGameState.PowerUps.ToList();
                    pul.Add(new PowerUpLocation(new Location(coord.X, coord.Y), (int)superPowerUp));
                    newGameState.PowerUps = pul.ToArray();
                }
            }

            return newGameState;
        }
    }
}