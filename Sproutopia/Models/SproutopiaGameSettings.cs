using Domain.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Sproutopia.Models
{
    public class SproutopiaGameSettings : AppSettings
    {
        [JsonPropertyName("Rows")]
        [Description("Height of game world")]
        public int Rows { get; set; }

        [JsonPropertyName("Cols")]
        [Description("Width of game world")]
        public int Cols { get; set; }

        [JsonPropertyName("Seed")]
        [Description("Seed value for deterministic randomization")]
        public int Seed { get; set; }

        [JsonPropertyName("NumberOfPlayers")]
        [Description("Number of bots per game")]
        [Range(1, 4, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int NumberOfPlayers { get; set; }

        [JsonPropertyName("TickRate")]
        [Description("Frequency (in milliseconds) of ticks")]
        [Range(50, 250, ErrorMessage = "Value for {0} must be between {1} and {2} ms.")]
        public int TickRate { get; set; }

        [JsonPropertyName("PlayerWindowSize")]
        [Description("Size of the game world visible to each player")]
        [Range(8, 10, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int PlayerWindowSize { get; set; }

        [JsonPropertyName("PlayerQueueSize")]
        [Description("Number of commands that can be queued up by each player")]
        [Range(3, 5, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int PlayerQueueSize { get; set; }

        [JsonPropertyName("MaxTicks")]
        [Description("Duration of game if other game over conditions aren't met earlier")]
        public int MaxTicks { get; set; }

        [JsonPropertyName("WeedsStartTick")]
        [Description("Tick counter before random weed spawning is introduced")]
        public int WeedsStartTick { get; set; }

        [JsonPropertyName("WeedSpawnRateMean")]
        [Description("Average number of ticks to lapse before new weed spawns")]
        public int WeedSpawnRateMean { get; set; }

        [JsonPropertyName("WeedSpawnRateMin")]
        [Description("Minimum number of ticks to lapse before new weed spawns")]
        public int WeedSpawnRateMin { get; set; }

        [JsonPropertyName("WeedSpawnRateMax")]
        [Description("Maximum number of ticks to lapse before new weed spawns")]
        public int WeedSpawnRateMax { get; set; }

        [JsonPropertyName("WeedSpawnRateStdDev")]
        [Description("Standard deviation for normal distribution of weed spawning")]
        public int WeedSpawnRateStdDev { get; set; }

        [JsonPropertyName("WeedsMaxAmount")]
        [Description("Maximum number of weeds to allow at once")]
        public int WeedsMaxAmount { get; set; }

        [JsonPropertyName("WeedsMaxGrowth")]
        [Description("Maximum size that weeds will grow to")]
        public int WeedsMaxGrowth { get; set; }

        [JsonPropertyName("WeedGrowthRate")]
        [Description("Number of ticks to pass for each growth increment of weeds")]
        public int WeedGrowthRate { get; set; }

        [JsonPropertyName("PowerupStartTick")]
        [Description("Tick counter before random powerup spawning is introduced")]
        public int PowerUpStartTick { get; set; }

        [JsonPropertyName("PowerUpSpawnRateMean")]
        [Description("Average number of ticks to lapse before new powerup spawns")]
        public int PowerUpSpawnRateMean { get; set; }

        [JsonPropertyName("PowerUpSpawnRateMin")]
        [Description("Minimum number of ticks to lapse before new powerup spawns")]
        public int PowerUpSpawnRateMin { get; set; }

        [JsonPropertyName("PowerUpSpawnRateMax")]
        [Description("Maximum number of ticks to lapse before new powerup spawns")]
        public int PowerUpSpawnRateMax { get; set; }

        [JsonPropertyName("PowerUpSpawnRateStdDev")]
        [Description("Standard deviation for normal distribution of powerup spawning")]
        public int PowerUpSpawnRateStdDev { get; set; }

        [JsonPropertyName("PowerUpsMaxAmount")]
        [Description("Maximum number of powerups to allow at once")]
        public int PowerUpsMaxAmount { get; set; }

        [JsonPropertyName("LifespanImmunity")]
        [Description("Duration (in ticks) of Temporary Territory Immunity powerup")]
        public int LifespanImmunity { get; set; }

        [JsonPropertyName("LifespanUnprunable")]
        [Description("Duration (in ticks) of Unprunable powerup")]
        public int LifespanUnprunable { get; set; }

        [JsonPropertyName("LifespanFreeze")]
        [Description("Duration (in ticks) of Freeze powerup")]
        public int LifespanFreeze { get; set; }

        [JsonPropertyName("LifespanFertilizer")]
        [Description("Duration (in ticks) of Super Fertilizer powerup")]
        public int LifespanFertilizer { get; set; }

        [JsonPropertyName("InputLogFile")]
        [Description("File to be used for replaying bot inputs")]
        public string InputLogFile { get; set; }

        [JsonPropertyName("DifferentialLoggingEnabled")]
        [Description("Enable differentail logging")]
        public bool DifferentialLoggingEnabled { get; set; }

        [JsonPropertyName("FullLoggingEnabled")]
        [Description("Enable full logging")]
        public bool FullLoggingEnabled { get; set; }
    }

}
