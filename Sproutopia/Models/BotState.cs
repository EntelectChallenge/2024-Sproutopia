using Domain;
using Domain.Models;
using Serilog;
using Sproutopia.Enums;

namespace Sproutopia.Models
{
    public class BotState : IBotState
    {
        public readonly string ConnectionId;
        public readonly string Nickname;
        private Queue<SproutBotCommand> _commandQueue;
        public SproutBotCommand LastCommand { get; private set; }
        public BotAction Momentum { get; private set; }
        public Guid BotId { get; private set; }
        public CellCoordinate Position { get; private set; }
        public CellCoordinate RespawnPosition { get; set; }
        private PowerUpType? _powerUpActive { get; set; } = null;
        private int _powerUpCountdown { get; set; } = 0;
        private SuperPowerUpType? _superPowerUpActive { get; set; } = null;
        private int _superPowerUpCountdown { get; set; } = 0;
        public int TieBreakingPoints { get; set; } // TODO: Still uncertain how this will be calculate and when and by whom it will be set

        public BotState(Guid botId, string nickname, string connectionId, CellCoordinate position)
        {
            _commandQueue = new();
            BotId = botId;
            LastCommand = new(botId, BotAction.IDLE);
            Momentum = BotAction.IDLE;
            TieBreakingPoints = 0;
            ConnectionId = connectionId;
            Nickname = nickname;
            Position = RespawnPosition = position;
        }
        public BotState(Guid botId, CellCoordinate position)
        {
            _commandQueue = new();
            BotId = botId;
            LastCommand = new(botId, BotAction.IDLE);
            Momentum = BotAction.IDLE;
            TieBreakingPoints = 0;
            Position = RespawnPosition = position;
        }

        public void SetPosition(CellCoordinate position, BotAction momentum)
        {
            Position = position;
            Momentum = momentum;
        }

        /// <summary>
        /// Returns whether specified powerup is active on the bot
        /// </summary>
        /// <param name="powerUpType">Type of powerup to check</param>
        /// <returns>boolean</returns>
        public bool IsActive(PowerUpType powerUpType) => _powerUpActive == powerUpType;

        /// <summary>
        /// Returns whether specified super powerup is active on the bot
        /// </summary>
        /// <param name="superPowerUpType">Type of super powerup to check</param>
        /// <returns>boolean</returns>
        public bool IsActive(SuperPowerUpType superPowerUpType) => _superPowerUpActive == superPowerUpType;

        /// <summary>
        /// Sets the active powerup to specified value
        /// </summary>
        /// <param name="powerUpType">Value of powerup</param>
        /// <param name="lifespan">Duration in ticks for powerup to remain active</param>
        public void SetActive(PowerUpType? powerUpType, int lifespan)
        {
            _powerUpActive = powerUpType;
            _powerUpCountdown = lifespan;
        }

        /// <summary>
        /// Sets the active super powerup to specified value
        /// </summary>
        /// <param name="superPowerUpType">Value of super powerup</param>
        /// <param name="lifespan">Duration in ticks for super powerup to remain active</param>
        public void SetActive(SuperPowerUpType? superPowerUpType, int lifespan)
        {
            _superPowerUpActive = superPowerUpType;
            _superPowerUpCountdown = lifespan;
        }

        public PowerUpType? GetActivePowerUp()
        {
            return _powerUpActive;
        }

        public SuperPowerUpType? GetActiveSuperPowerUp()
        {
            return _superPowerUpActive;
        }

        /// <summary>
        /// Decrements the countdown timer on any active powerups and expire powerup if counter reaches zero
        /// </summary>
        public void PowerupsCountdown()
        {
            if (_powerUpActive != null && --_powerUpCountdown <= 0)
                ClearActivePowerUp();

            if (_superPowerUpActive != null && --_superPowerUpCountdown <= 0)
                ClearActiveSuperPowerUp();
        }

        /// <summary>
        /// Disables active powerup
        /// </summary>
        public void ClearActivePowerUp()
        {
            _powerUpActive = null;
        }

        /// <summary>
        /// Disables active super powerup
        /// </summary>
        public void ClearActiveSuperPowerUp()
        {
            _superPowerUpActive = null;
        }

        /// <summary>
        /// Enqueues a new command onto the command queue if the command is valid
        /// </summary>
        /// <param name="command">The command to be enqueued</param>
        /// <returns>Boolean value determining whether the command was enqueued successfully</returns>
        public Task<bool> EnqueueCommand(SproutBotCommand command)
        {
            switch (LastCommand.Action)
            {
                case BotAction.Up: if (command.Action == BotAction.Down) return Task.FromResult(false); break;
                case BotAction.Down: if (command.Action == BotAction.Up) return Task.FromResult(false); break;
                case BotAction.Left: if (command.Action == BotAction.Right) return Task.FromResult(false); break;
                case BotAction.Right: if (command.Action == BotAction.Left) return Task.FromResult(false); break;
            }

            _commandQueue.Enqueue(command);
            LastCommand = command;

            return Task.FromResult(true);
        }

        /// <summary>
        /// Dequeues and returns the next command on the command queue, or the last command if the queue is empty
        /// </summary>
        /// <returns>SproutBotCommand</returns>
        public SproutBotCommand DequeueCommand()
        {
            if (!_commandQueue.TryDequeue(out var command))
            {
                Log.Debug($"{BotId}: PROCESSING Empty Queue taking last Command {LastCommand.Action}");
                command = LastCommand;
            }

            return command;
        }

        /// <summary>
        /// Clears the bot's command queue and set the last command to idle
        /// </summary>
        public void ClearQueue()
        {
            _commandQueue.Clear();
            LastCommand = new(BotId, BotAction.IDLE);
            Momentum = BotAction.IDLE;
        }
    }
}
