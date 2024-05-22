using Domain.Enums;
using Domain.Models;
using Logger.Utilities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Runner.Services;
using Serilog;
using Sproutopia.Managers;
using Sproutopia.Models;
using Sproutopia.Utilities;
using System.Text.Json;

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
        private Serilog.Core.Logger _inputLogger;
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
            IHubContext<RunnerHub> runnerContext,
            Serilog.Core.Logger inputLogger)
        {
            _gameState = gameState;
            _gameSettings = settings.Value;
            _tickManager = tickManager;
#if DEBUG
            _visualiserContext = visualiserContext;
#endif
            _runnerContext = runnerContext;
            _randomizer = randomizer;
            _inputLogger = inputLogger;
            _cloudIntegrationService = cloudIntegrationService;

            var LOG_DIRECTORY = Environment.GetEnvironmentVariable("LOG_DIR") ?? Path.Combine(AppContext.BaseDirectory[..AppContext.BaseDirectory.IndexOf("Sproutopia")], "Logs");

            var filename = DateTime.Now.ToString("yyMMddHHmmss");

            _endGameLogger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "Sproutopia")
                .MinimumLevel.Information()
                .WriteTo.File(Path.Combine(LOG_DIRECTORY, $"{filename}endGame.json"), outputTemplate: "{Message:lj}{NewLine}")
                .CreateLogger();

            _gameLogger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Information()
                .WriteTo.File(Path.Combine(LOG_DIRECTORY, $"{filename}.log"), outputTemplate: "{Message:lj}{NewLine}")
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
            finally
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    _gameLogger.Error("Worker stopped unexpectedly");
                }
            }
        }

        private async Task GameLoop(CancellationToken stoppingToken)
        {
            Dictionary<int, List<SproutBotCommand>> tickCommands = [];

            // Read input log if provided
            if (!string.IsNullOrEmpty(_gameSettings.InputLogFile))
            {
                List<SproutBotCommand> currentCommands = [];
                int currentTick = 0;

                foreach (var line in File.ReadLines(_gameSettings.InputLogFile))
                {
                    var trimLine = line.Split(" - ")[1];

                    if (trimLine.StartsWith("Register"))
                    {
                        var registerElements = trimLine.Split(',');
                        _gameState.AddBot(Guid.Parse(registerElements[1]), registerElements[2], Guid.NewGuid().ToString());
                    }

                    if (trimLine.StartsWith("Tick"))
                    {
                        currentTick = int.Parse(trimLine.Split(',')[1]);
                        tickCommands.Add(currentTick, currentCommands);
                        currentCommands = [];
                    }

                    if (trimLine.StartsWith("Command"))
                    {
                        var botId = Guid.Parse(trimLine.Split(',')[1]);
                        var cmd = trimLine.Split(',')[2] switch
                        {
                            "Up" => BotAction.Up,
                            "Down" => BotAction.Down,
                            "Left" => BotAction.Left,
                            "Right" => BotAction.Right,
                            _ => BotAction.IDLE,
                        };

                        currentCommands.Add(new SproutBotCommand(botId, cmd));
                    }
                }

                Console.WriteLine("Ready to start replaying input log. If you want to connect a visualizer first, now is a good time.");
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
                _tickManager.StartTimer();
            }

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

            var prevGameState = new DiffLog();

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_tickManager.ShouldContinue())
                {
                    if (!_gameState.GameOver)
                    {
                        Log.Debug($"TICK: {_tickManager.CurrentTick}");
                        _inputLogger.Information($"Tick,{_tickManager.CurrentTick}");

                        // Add commands for current tick from input log (if provided) to bot queues
                        if (tickCommands.TryGetValue(_tickManager.CurrentTick, out var newCommands))
                        {
                            foreach (var command in newCommands)
                            {
                                await _gameState.BotManager.EnqueueCommand(command);
                            }
                        }

                        var respawnList = new List<Guid>();
                        var interruptedList = new List<Guid>();
                        _gameState.BotSnapshots = [];

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
                            _gameState.BotSnapshots.Add(
                                new BotSnapshot(
                                    timeStamp: command.TimeStamp,
                                    botId: command.BotId,
                                    action: command.Action,
                                    momentum: _gameState.BotManager.GetBotState(command.BotId).Momentum));

                            if (respawnList.Contains(command.BotId) || interruptedList.Contains(command.BotId))
                                continue;

                            // Previously, the check for a valid command was performed when a command is enqueued but that is incorrect as commands can be
                            // queued up faster than they are dequeued (which is kinda the point of a queue). To check the validity of a future command
                            // against the current bot state would be wrong so the logic has been moved here.
                            var momentum = _gameState.BotManager.GetBotState(command.BotId).Momentum;
                            if (command.Action == ExtensionMethods.Reverse(momentum))
                                command.Action = momentum;

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
                        var currentPath = Directory.GetCurrentDirectory();
                        var filePath = Path.Combine(currentPath, "SampleFullGameLogsOutput.json");
                        Helpers.WriteJsonToFile(filePath, JsonSerializer.Serialize(_gameState.MapAllToDto()));
                        await _visualiserContext.Clients.All.SendAsync(VisualiserCommands.ReceiveInitialGameState,
                            _gameState.MapAllToDto());
#endif

                        await Parallel.ForEachAsync(_gameState.BotManager.GetAllBotStates(),
                            async (bot, cancellationToken) =>
                            {
                                await _runnerContext.Clients.Client(bot.Value.ConnectionId)
                                    .SendAsync(RunnerCommands.ReceiveBotState, _gameState.MapToBotDto(bot.Key),
                                        cancellationToken);
                                _cloudIntegrationService.UpdatePlayer(bot.Key.ToString());
                            });

                        if (_gameSettings.DifferentialLoggingEnabled)
                        {

                            var diffLog = _gameState.MapToDiffLog(prevGameState);
                            prevGameState = _gameState.MapToDiffLog(new DiffLog()); // last remembered state has to be absolute, not relative

                            _gameLogger.Information("{@_gameState}", Newtonsoft.Json.JsonConvert.SerializeObject(diffLog)); // System.Text.Json does not render this object correctly, hence Newtonsoft
                        }
                        else
                        {
                            _gameLogger.Information("{@_gameState}", Newtonsoft.Json.JsonConvert.SerializeObject(_gameState.MapAllToDto())); // System.Text.Json does not render this object correctly, hence Newtonsoft
                        }
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