using Domain.Enums;
using Domain.Models;
using Logger.Utilities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Runner.Services;
using Serilog;
using Serilog.Sinks.File.GZip;
using Sproutopia.Managers;
using Sproutopia.Models;

namespace Sproutopia
{
    public class EngineWorker : BackgroundService
    {
        private readonly SproutopiaGameSettings _gameSettings;
        private readonly GameState _gameState;
        private readonly ITickManager _tickManager;
        private readonly IHubContext<RunnerHub> _runnerContext;
        private readonly ICloudIntegrationService _cloudIntegrationService;
        private readonly GlobalSeededRandomizer _randomizer;
        private Serilog.Core.Logger _endGameLogger;
        private Serilog.Core.Logger _gameLogger;
#if DEBUG
        private readonly IHubContext<VisualiserHub> _visualiserContext;
#endif

        public EngineWorker(
            IOptions<SproutopiaGameSettings> settings,
            GameState gameState,
            ITickManager tickManager,
            IHubContext<VisualiserHub> visualiserContext,
            ICloudIntegrationService cloudIntegrationService,
            GlobalSeededRandomizer randomizer,
            IHubContext<RunnerHub> runnerContext
        )
        {
            _gameState = gameState;
            _gameSettings = settings.Value;
            _tickManager = tickManager;
#if DEBUG
            _visualiserContext = visualiserContext;
#endif
            _runnerContext = runnerContext;
            _randomizer = randomizer;
            _cloudIntegrationService = cloudIntegrationService;

            _endGameLogger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "Sproutopia")
                .MinimumLevel.Information()
                .WriteTo.File("logs\\engGame.json")
                .CreateLogger();

            _gameLogger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Information()
                .WriteTo.File("log.gz", hooks: new GZipHooks(), outputTemplate: "{Message}{NewLine}{Expression}")
                .CreateLogger();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await GameLoop(stoppingToken);
            }
            catch (Exception e)
            {
                Log.Error($"Game failed with exception: {e.Message}");
                await HandleCriticalException(e);
            }
        }

        private async Task GameLoop(CancellationToken stoppingToken)
        {
            var nextWeedSpawn = (int)_randomizer.NextNormal(
                _gameSettings.WeedSpawnRateMean,
                _gameSettings.WeedSpawnRateStdDev,
                _gameSettings.WeedSpawnRateMin,
                _gameSettings.WeedSpawnRateMax);

            var nextPowerUpSpawn = (int)_randomizer.NextNormal(
                _gameSettings.PowerUpSpawnRateMean,
                _gameSettings.PowerUpSpawnRateStdDev,
                _gameSettings.PowerUpSpawnRateMin,
                _gameSettings.PowerUpSpawnRateMax);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_tickManager.ShouldContinue())
                {
                    if (!_gameState.GameOver)
                    {
                        Log.Debug($"TICK: {_tickManager.CurrentTick}");

                        var respawnList = new List<Guid>();
                        var interruptedList = new List<Guid>();

                        #region STAGE 1

                        // STAGE 1
                        // For each bot CommandQueue
                        // Pop one command off the queue
                        // Sort by timestamp
                        var sproutBotCommands = new List<SproutBotCommand>();
                        foreach (var (_, botState) in _gameState.BotManager.GetAllBotStates())
                        {
                            Log.Debug($"{botState.BotId}: PROCESSING First Command In Queue");
                            sproutBotCommands.Add(botState.DequeueCommand());
                        }

                        sproutBotCommands.Sort();

                        #endregion

                        #region STAGE 2

                        // STAGE 2
                        // For Each bot
                        // Check if bot is in the respawn list
                        // TRUE: skip to next bot
                        //
                        // Validate the command (or continuation of current movement if no command received)
                        //   Is direction opposite to CurrentMovement?
                        // FALSE: skip to next bot
                        //
                        // Inform GardenManager of bot's action
                        // BotManager will calculate and apply all side effects resulting from bot action and return bot's new position, any powerups collected, a list of bots
                        // that were pruned during the action, and a list of bots that were interrupted (pruned but for them having a protective powerup active)

                        foreach (var command in sproutBotCommands)
                        {
                            if (respawnList.Contains(command.BotId) || interruptedList.Contains(command.BotId))
                                continue;

                            // Checking if direction is valid is strictly no longer necessary because this check is performed when a command is enqueued
                            // But I'm leaving this comment here to remind myself that this is the point where this check would have to be performed if
                            // we decide to change this later.

                            var (pruned, interrupted) = _gameState.IssueCommand(command);
                            respawnList.AddRange(pruned.Except(respawnList));
                            interruptedList.AddRange(interrupted.Except(interruptedList));
                        }

                        #endregion

                        #region STAGE 3

                        // STAGE 3
                        // Respawn bots flagged for respawn (instruction to GardenManager)
                        foreach (var prunedBot in respawnList)
                        {
                            _gameState.RespawnBot(prunedBot);
                        }

                        #endregion

                        #region STAGE 4

                        // STAGE 4
                        // Decide whether or not to spawn a power-up or grow a weed

                        // Weeds
                        if (_tickManager.CurrentTick >= _gameSettings.WeedsStartTick)
                        {
                            _gameState.GrowWeeds();

                            if (--nextWeedSpawn <= 0)
                            {
                                _gameState.AddWeed();
                                nextWeedSpawn = (int)_randomizer.NextNormal(
                                    _gameSettings.WeedSpawnRateMean,
                                    _gameSettings.WeedSpawnRateStdDev,
                                    _gameSettings.WeedSpawnRateMin,
                                    _gameSettings.WeedSpawnRateMax);
                            }
                        }


                        // PowerUps
                        if (_tickManager.CurrentTick >= _gameSettings.PowerUpStartTick)
                        {
                            if (--nextPowerUpSpawn <= 0)
                            {
                                _gameState.AddPowerUp();
                                nextPowerUpSpawn = (int)_randomizer.NextNormal(
                                    _gameSettings.PowerUpSpawnRateMean,
                                    _gameSettings.PowerUpSpawnRateStdDev,
                                    _gameSettings.PowerUpSpawnRateMin,
                                    _gameSettings.PowerUpSpawnRateMax);
                            }
                        }

                        #endregion

                        #region STAGE 5

                        // STAGE 5
                        // Send feedback to bots & Visualiser
                        // Update GameState and BotState
                        _gameState.UpdateState(_tickManager.CurrentTick);
#if DEBUG
                        await _visualiserContext.Clients.All.SendAsync(VisualiserCommands.ReceiveInitialGameState,
                            _gameState.MapAllToDto());
#endif

                        await Parallel.ForEachAsync(_gameState.BotManager.GetAllBotStates(), async (bot, cancellationToken) => {
                            await _runnerContext.Clients.Client(bot.Value.ConnectionId).SendAsync(
                                    RunnerCommands.ReceiveBotState,
                                    _gameState.MapToBotDto(bot.Key),
                                    cancellationToken
                            );
                        });

                        _gameLogger.Information("{@_gameState}", _gameState.MapAllToDto());

                        #endregion
                    }
                }

                if (_gameState.CurrentTick >= _gameSettings.MaxTicks)
                {
                    if (_gameState.GameOver)
                    {
                        await TriggerEndGame(stoppingToken);
                    }
                }

                await Task.Delay(1);
            }
        }

        public async Task TriggerEndGame(CancellationToken stoppingToken)
        {
            Log.Information("Game Complete");

            /*            Parallel.ForEach(_gameState.BotManager.GetAllBotStates(), bot =>
                        {
                            bot.TotalPoints += bot.Hero.Collected;
                        });*/

            var leaderBoard = _gameState.GardenManager.Leaderboard().OrderByDescending(gs => gs.claimedPercentage);
            var bots = _gameState.BotManager.GetAllBotStates();

            var gameComplete = new GameComplete
            {
                TotalTicks = _gameState.CurrentTick,
                Players = leaderBoard.Select((bot, index) =>
                    new PlayerResult
                    {
                        Placement = index + 1,
                        Score = bot.claimedPercentage,
                        Id = bot.botId.ToString(),
                        Nickname = bots[bot.botId].Nickname,
                        MatchPoints = leaderBoard.Count() - index
                    }).ToList(),
                WorldSeed = _gameSettings.Seed,
                WinngingBot = bots[leaderBoard.First().botId]
            };

            foreach (var posistion in leaderBoard.Select((bot, index) => new { bot, index }))
            {
                var currentBot = posistion.bot.botId;
                int matchPoint = gameComplete.Players.Find(p => string.Equals(p.Id, currentBot.ToString())).MatchPoints;
                int totalPoint = gameComplete.Players.Find(p => string.Equals(p.Id, currentBot.ToString())).Score;
                _cloudIntegrationService.UpdatePlayer(currentBot.ToString(), finalScore: totalPoint,
                    matchPoints: matchPoint, placement: posistion.index + 1);
            }

            // Log.File(gameComplete, null, "GameComplete");
            //Hand off ot Logger to handel end game state
            //Disconnect all bots
            await _runnerContext.Clients.All.SendAsync(RunnerCommands.EndGame, _gameSettings.Seed,
                _gameState.CurrentTick);
#if DEBUG
            await _visualiserContext.Clients.All.SendAsync("GameComplete", _gameSettings.Seed, _gameState.CurrentTick);
#endif

            #region End Game Logging

            _endGameLogger.Information("{@gameComplete}", gameComplete);

            #endregion

            await AnnounceCompletion();
            await StopAsync(stoppingToken);
        }

        private async Task AnnounceCompletion()
        {
            await Log.CloseAndFlushAsync();
            await _cloudIntegrationService.Announce(CloudCallbackType.Finished, null, _gameState.Seed,
                _gameState.CurrentTick);
            await S3.UploadLogs();
            await _cloudIntegrationService.Announce(CloudCallbackType.LoggingComplete);
        }

        private async Task HandleCriticalException(Exception e)
        {
            await Log.CloseAndFlushAsync();
            await _runnerContext.Clients.All.SendAsync(RunnerCommands.Disconnect,
                "A critical error prevented the game from completing");
            await _cloudIntegrationService.Announce(CloudCallbackType.Failed, e, _gameState.Seed,
                _gameState.CurrentTick);
            await S3.UploadLogs();
            await _cloudIntegrationService.Announce(CloudCallbackType.LoggingComplete);
        }
    }
}
