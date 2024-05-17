using Runner.DTOs;
using Sproutopia.Domain;
using Sproutopia.Utilities;

namespace Sproutopia.Models
{
    public class GameStateDto
    {
        public int CurrentTick { get; set; }
        public int[][] Land { get; set; }
        public Dictionary<Guid, BotStateDTO> Bots { get; set; }
        public Dictionary<CellType, CellCoordinate> ChangeLog { get; set; }
        public PowerUpLocation[] PowerUps { get; set; }
        public bool[][] Weeds { get; set; }

        public GameStateDto(int currentTick,
            int[][] land,
            Dictionary<Guid, BotStateDTO> bots,
            PowerUpLocation[] powerUps,
            bool[][] weeds)
        {
            CurrentTick = currentTick;
            Land = land;
            Bots = bots;
            ChangeLog = [];
            Weeds = weeds;
            PowerUps = powerUps;
        }

        public GameStateDto DeepCopy()
        {
            GameStateDto copy = new GameStateDto(
                currentTick : CurrentTick,
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