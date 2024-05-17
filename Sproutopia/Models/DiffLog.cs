using Domain.Enums;
using Sproutopia.Domain;
using Sproutopia.Enums;

namespace Sproutopia.Models
{
    public class DiffLog
    {
        public int CurrentTick { get; set; }
        public Dictionary<Guid, int> LeaderBoard { get; set; }
        public Dictionary<Guid, CellCoordinate> BotPositions { get; set; }
        public Dictionary<Guid, BotAction> BotDirections { get; set; }
        public Dictionary<Guid, PowerUpType> BotPowerUps { get; set; }
        public Dictionary<Guid, SuperPowerUpType> BotSuperPowerUps { get; set; }
        public Dictionary<CellCoordinate, int> Territory { get; set; }
        public Dictionary<CellCoordinate, int> Trails { get; set; }
        public Dictionary<CellCoordinate, bool> Weeds { get; set; }
        public Dictionary<CellCoordinate, PowerUpType> PowerUps { get; set; }
        public Dictionary<CellCoordinate, SuperPowerUpType> SuperPowerUps { get; set; }


        public DiffLog()
        {
            CurrentTick = 0;
            LeaderBoard = [];
            BotPositions = [];
            BotDirections = [];
            BotPowerUps = [];
            BotSuperPowerUps = [];
            Territory = [];
            Trails = [];
            Weeds = [];
            PowerUps = [];
            SuperPowerUps = [];
        }

        public DiffLog(
            int currentTick,
            Dictionary<Guid, int> leaderBoard,
            Dictionary<Guid, CellCoordinate> botPositions,
            Dictionary<Guid, BotAction> botDirections,
            Dictionary<Guid, PowerUpType> botPowerUps,
            Dictionary<Guid, SuperPowerUpType> botSuperPowerUps,
            Dictionary<CellCoordinate, int> territory,
            Dictionary<CellCoordinate, int> trails,
            Dictionary<CellCoordinate, bool> weeds,
            Dictionary<CellCoordinate, PowerUpType> powerUps,
            Dictionary<CellCoordinate, SuperPowerUpType> superPowerUps)
        {
            CurrentTick = currentTick;
            LeaderBoard = leaderBoard;
            BotPositions = botPositions;
            BotDirections = botDirections;
            BotPowerUps = botPowerUps;
            BotSuperPowerUps = botSuperPowerUps;
            Territory = territory;
            Trails = trails;
            Weeds = weeds;
            PowerUps = powerUps;
            SuperPowerUps = superPowerUps;
        }

        public DiffLog(DiffLog before, DiffLog after) : this(
                after.CurrentTick,
                GetChanges(before.LeaderBoard, after.LeaderBoard),
                GetChanges(before.BotPositions, after.BotPositions),
                GetChanges(before.BotDirections, after.BotDirections),
                GetChanges(before.BotPowerUps, after.BotPowerUps),
                GetChanges(before.BotSuperPowerUps, after.BotSuperPowerUps),
                GetChanges(before.Territory, after.Territory, (int)CellType.Unclaimed),
                GetChanges(before.Trails, after.Trails, (int)CellType.Unclaimed),
                GetChanges(before.Weeds, after.Weeds),
                GetChanges(before.PowerUps, after.PowerUps),
                GetChanges(before.SuperPowerUps, after.SuperPowerUps))
        {
        }

        public static Dictionary<TKey, TValue> GetChanges<TKey, TValue>(Dictionary<TKey, TValue> original, Dictionary<TKey, TValue> updated, TValue? removalValue = default(TValue)) where TKey : notnull
        {
            var changes = new Dictionary<TKey, TValue>();

            // Iterate over the keys in the original dictionary
            foreach (var key in original.Keys)
            {
                // Check if the key exists in the updated dictionary
                if (updated.TryGetValue(key, out TValue? value))
                {
                    // If the values are different, add the change to the result dictionary
                    if (!EqualityComparer<TValue>.Default.Equals(original[key], value))
                    {
                        changes[key] = value;
                    }
                }
                else
                {
                    // If the key does not exist in the updated dictionary, consider it as a removal
                    changes[key] = removalValue;
                }
            }

            // Iterate over the keys in the updated dictionary to find additions
            foreach (var key in updated.Keys)
            {
                // If the key does not exist in the original dictionary, consider it as an addition
                if (!original.ContainsKey(key))
                {
                    changes[key] = updated[key];
                }
            }

            return changes;
        }
    }
}