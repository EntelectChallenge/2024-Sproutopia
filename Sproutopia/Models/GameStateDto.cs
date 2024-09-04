using DeepCopy;
using Domain.Enums;
using Newtonsoft.Json;
using Runner.DTOs;
using Sproutopia.Enums;
using Sproutopia.Utilities;
using System.Text;

namespace Sproutopia.Models
{
    public class GameStateDto
    {
        public DateTime TimeStamp { get; set; }
        public int CurrentTick { get; set; }
        public List<BotSnapshot> BotSnapshots { get; set; } = [];

        [JsonIgnore]
        public int[][] Territory { get; set; }

        [JsonIgnore]
        public int[][] Trails { get; set; }

        public int[][] Land
        {
            // Land is derived from the combination of Territory and Trails
            get
            {
                var land = Helpers.CreateJaggedArray<int[][]>(Territory.Length, Territory[0].Length);

                for (var x = 0; x < Territory.Length; x++)
                    for (var y = 0; y < Territory[x].Length; y++)
                        land[x][y] = Trails[x][y] == 255 ? Territory[x][y] : Trails[x][y];

                return land;
            }

            // Conversely, if Land is set explicitly (such as when being read from a log file),
            // Territory and Trails must be back populated accordingly
            set
            {
                Territory = Helpers.CreateJaggedArray<int[][]>(value.Length, value[0].Length);
                Trails = Helpers.CreateJaggedArray<int[][]>(value.Length, value[0].Length);

                for (var x = 0; x < value.Length; x++)
                    for (var y = 0; y < value[x].Length; y++)
                    {
                        switch (value[x][y])
                        {
                            case int i when i >= 0 && i < 4:
                                Territory[x][y] = i;
                                Trails[x][y] = 255;
                                break;
                            case int i when i >= 4 && i < 8:
                                Territory[x][y] = 255;
                                Trails[x][y] = i;
                                break;
                            default:
                                Territory[x][y] = 255;
                                Trails[x][y] = 255;
                                break;
                        }
                    }
            }
        }
        public List<(Guid botId, int percentage)> LeaderBoard { get; set; } = [];

        [JsonIgnore]
        public List<(string botName, int percentage)> LeaderBoardByName =>
            LeaderBoard
            .Select(kvp => (BotSnapshots.FirstOrDefault(b => b.BotId == kvp.botId).BotName, kvp.percentage))
            .ToList();

        public PowerUpLocation[] PowerUps { get; set; } = [];
        public bool[][] Weeds { get; set; }

        public GameStateDto(
            DateTime timeStamp,
            int currentTick,
            List<BotSnapshot> botSnapshots,
            int[][] territory,
            int[][] trails,
            List<(Guid botId, int percentage)> leaderBoard,
            PowerUpLocation[] powerUps,
            bool[][] weeds)
        {
            TimeStamp = timeStamp;
            CurrentTick = currentTick;
            //BotSnapshots = botSnapshots.OrderBy(x => x.BotId).ToList();
            BotSnapshots = botSnapshots;
            Territory = territory;
            Trails = trails;
            //LeaderBoard = leaderBoard.OrderBy(x => x.botId).ToList();
            LeaderBoard = leaderBoard;
            Weeds = weeds;
            PowerUps = powerUps;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Tick: {CurrentTick}");
            sb.AppendLine();
            foreach (var bot in BotSnapshots)
            {
                sb.AppendLine($"{bot.BotId.ToString().Substring(0, 4)}...: {bot.X},{bot.Y}->{bot.Action}({bot.Momentum})");
            }
            sb.AppendLine();
            sb.AppendLine("Power Ups:");
            foreach (var bot in BotSnapshots)
            {
                sb.AppendLine($"{bot.BotId.ToString().Substring(0, 4)}...: {bot.PowerUp}");
            }
            sb.AppendLine();
            sb.AppendLine("Super Power Ups:");
            foreach (var bot in BotSnapshots)
            {
                sb.AppendLine($"{bot.BotId.ToString().Substring(0, 4)}...: {bot.SuperPowerUp}");
            }

            return sb.ToString();
        }

        public GameStateDto DeepCopy()
        {
            return DeepCopier.Copy(this);
        }

        public GameStateDto DeepCopy_Old()
        {
            // Using Json serialization/deserialization for deep copy as a quick and dirty alternative for the deep copy code I wrote before
            // as that had some bugs which I'm too lazy to figure out now. Besides, this seems to be fast enough not to be a problem.
            return JsonConvert.DeserializeObject<GameStateDto>(JsonConvert.SerializeObject(this));
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
            newGameState.TimeStamp = diffLog.TimeStamp;
            newGameState.CurrentTick = diffLog.CurrentTick;

            foreach (var (key, botPercentage) in diffLog.LeaderBoard)
            {
                newGameState.LeaderBoard.RemoveAll(b => b.botId == key);
                newGameState.LeaderBoard.Add((key, botPercentage));
            }
            newGameState.LeaderBoard.Sort((a, b) => a.botId.CompareTo(b.botId));

            var index = 0;
            foreach (var (key, position) in diffLog.BotPositions)
            {
                var snapshot = newGameState.BotSnapshots.Find(b => b.BotId == key);
                snapshot ??= new BotSnapshot(
                    index: index,
                    botId: key,
                    botName: $"Bot{index}",
                    action: BotAction.IDLE,
                    momentum: BotAction.IDLE,
                    powerUp: PowerUpType.NONE,
                    superPowerUp: SuperPowerUpType.NONE,
                    x: position.X,
                    y: position.Y);
                snapshot.X = position.X;
                snapshot.Y = position.Y;

                newGameState.BotSnapshots.RemoveAll(b => b.BotId == key);
                newGameState.BotSnapshots.Add(snapshot);

                index = snapshot.Index + 1;
            }

            index = 0;
            foreach (var (key, botDir) in diffLog.BotDirections)
            {
                var snapshot = newGameState.BotSnapshots.Find(b => b.BotId == key);
                snapshot ??= new BotSnapshot(
                    index: index,
                    botId: key,
                    botName: $"Bot{index}",
                    action: botDir,
                    momentum: BotAction.IDLE,
                    powerUp: PowerUpType.NONE,
                    superPowerUp: SuperPowerUpType.NONE,
                    x: 0,
                    y: 0);
                snapshot.Action = botDir;

                newGameState.BotSnapshots.RemoveAll(b => b.BotId == key);
                newGameState.BotSnapshots.Add(snapshot);

                index = snapshot.Index + 1;
            }

            index = 0;
            foreach (var (key, botMomentum) in diffLog.BotMomentums)
            {
                var snapshot = newGameState.BotSnapshots.Find(b => b.BotId == key);
                snapshot ??= new BotSnapshot(
                    index: index,
                    botId: key,
                    botName: $"Bot{index}",
                    action: BotAction.IDLE,
                    momentum: botMomentum,
                    powerUp: PowerUpType.NONE,
                    superPowerUp: SuperPowerUpType.NONE,
                    x: 0,
                    y: 0);
                snapshot.Momentum = botMomentum;

                newGameState.BotSnapshots.RemoveAll(b => b.BotId == key);
                newGameState.BotSnapshots.Add(snapshot);

                index = snapshot.Index + 1;
            }

            index = 0;
            foreach (var (key, botPowerUp) in diffLog.BotPowerUps)
            {
                var snapshot = newGameState.BotSnapshots.Find(b => b.BotId == key);
                snapshot ??= new BotSnapshot(
                    index: index,
                    botId: key,
                    botName: $"Bot{index}",
                    action: BotAction.IDLE,
                    momentum: BotAction.IDLE,
                    powerUp: botPowerUp,
                    superPowerUp: SuperPowerUpType.NONE,
                    x: 0,
                    y: 0);
                snapshot.PowerUp = botPowerUp;

                newGameState.BotSnapshots.RemoveAll(b => b.BotId == key);
                newGameState.BotSnapshots.Add(snapshot);

                index = snapshot.Index + 1;
            }

            index = 0;
            foreach (var (key, botSuperPowerUp) in diffLog.BotSuperPowerUps)
            {
                var snapshot = newGameState.BotSnapshots.Find(b => b.BotId == key);
                snapshot ??= new BotSnapshot(
                    index: index,
                    botId: key,
                    botName: $"Bot{index}",
                    action: BotAction.IDLE,
                    momentum: BotAction.IDLE,
                    powerUp: PowerUpType.NONE,
                    superPowerUp: botSuperPowerUp,
                    x: 0,
                    y: 0);
                snapshot.SuperPowerUp = botSuperPowerUp;

                newGameState.BotSnapshots.RemoveAll(b => b.BotId == key);
                newGameState.BotSnapshots.Add(snapshot);

                index = snapshot.Index + 1;
            }
            newGameState.BotSnapshots.Sort((a, b) => a.Index.CompareTo(b.Index));

            foreach (var (coord, territory) in diffLog.Territory)
            {
                newGameState.Territory[coord.X][coord.Y] = territory;
            }

            foreach (var (coord, trail) in diffLog.Trails)
            {
                newGameState.Trails[coord.X][coord.Y] = trail == 255 ? 255 : trail + 4;
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

            newGameState.PowerUps = newGameState.PowerUps.OrderBy(pu => pu.Location).ToArray();

            return newGameState;
        }
    }
}