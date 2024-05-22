using Domain.Enums;
using Domain.Interfaces;
using Domain.Models;
using Logger;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using Runner.Services;
using Serilog;
using Sproutopia.Managers;
using Sproutopia.Models;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Sproutopia
{
    public class SproutopiaEngine : IEngine
    {
        private readonly GlobalSeededRandomizer _randomizer;
        private readonly IHubContext<RunnerHub> _runnerContext;
#if DEBUG
        private readonly IHubContext<VisualiserHub> _visualiserContext;
#endif
        private readonly ICloudIntegrationService _cloudIntegrationService;
        private HubConnection _hubConnection;
        private readonly ITickManager _tickManager;
        private readonly SproutopiaGameSettings _gameSettings;
        private GameState _gameState;
        public bool IsRunning { get; set; } = false;

        public static Timer ConnectionTimer;
        private Serilog.Core.Logger _inputLogger;


        IStreamingFileLogger IEngine.StateLogger => new StreamingFileLogger(DateTime.Now.ToString("yy-MM-dd-THHmm"));
        IStreamingFileLogger IEngine.GameCompleteLogger => new StreamingFileLogger("GameComplete");

        public bool IsBotAuthorized(Guid botId, string connectionId) =>
            _gameState.BotManager.IsBotRegistered(botId) &&
            _gameState.BotManager.IsBotConnectionValid(botId, connectionId);


        public bool IsStartConditionsMet() => _gameState.BotManager.BotCount() == _gameSettings.NumberOfPlayers;

        public SproutopiaEngine(
            IOptions<SproutopiaGameSettings> settings,
            IHubContext<RunnerHub> runnerContext,
#if DEBUG
            IHubContext<VisualiserHub> visualiserContext,
#endif
                        ITickManager tickManager,
                        GameState gameState,
                        GlobalSeededRandomizer randomizer,
                        ICloudIntegrationService cloudIntegrationService,
                        Serilog.Core.Logger inputLogger)
        {
            _randomizer = randomizer;
            _tickManager = tickManager;
#if DEBUG
            _visualiserContext = visualiserContext;
#endif
            _runnerContext = runnerContext;
            _gameSettings = settings.Value;
            _gameState = gameState;
            _inputLogger = inputLogger;
            _cloudIntegrationService = cloudIntegrationService;


            int timeoutMillis = int.TryParse(Environment.GetEnvironmentVariable("BOT_TIMEOUT"), out timeoutMillis) ? timeoutMillis : 300000; // 5 minutes
            ConnectionTimer = new(timeoutMillis);
            ConnectionTimer.Elapsed += BotConnectionTimeout;
            ConnectionTimer.AutoReset = false;
            ConnectionTimer.Enabled = true;
        }

        public HubConnection SetHubConnection(ref HubConnection connection) => _hubConnection = connection;

        public async Task AddCommandToBotQueue(BotCommand botCommand)
        {
            var sproutBotCommand = new SproutBotCommand(botCommand.BotId, (BotAction)botCommand.Action);

            if (sproutBotCommand.Action.Equals(null) || sproutBotCommand.Action == BotAction.IDLE) return;

            Log.Debug($"{sproutBotCommand.BotId}: ADDING command {sproutBotCommand.Action} to Queue");
            _inputLogger.Information($"Command,{sproutBotCommand.BotId},{sproutBotCommand.Action}");

            await _gameState.BotManager.EnqueueCommand(sproutBotCommand);
        }

        public async Task RequestGameInfo(Guid botId)
        {
            var bot = _gameState.BotManager.GetBotState(botId);
            if (bot != null)
            {
                var dto = _gameState.MapToGameInfoDto();
                await _runnerContext.Clients.Client(bot.ConnectionId)
                    .SendAsync(RunnerCommands.ReceiveGameInformation, dto);
            }
        }

        public void RegisterBot(Guid token, string nickName, string connectionId)
        {
            _inputLogger.Information($"Register,{token},{nickName}");
            var registeredBotId = _gameState.AddBot(token, nickName, connectionId);
            _cloudIntegrationService.AddPlayer(0, registeredBotId.ToString(), 0, 0, registeredBotId.ToString());
        }


        public async Task StartGame()
        {
            try
            {
                await _runnerContext.Clients.All.SendAsync(RunnerCommands.ReceiveGameInformation, _gameState.MapToGameInfoDto());
                _tickManager.StartTimer();
#if DEBUG
                await _visualiserContext.Clients.All.SendAsync(VisualiserCommands.ReceiveInitialGameState, _gameState.MapAllToDto());
#endif
            }
            catch (Exception ex) // Handle any uncaught exceptions
            {
                HandleCriticalException(ex).RunSynchronously();
            }
        }

        private async Task HandleCriticalException(Exception ex)
        {
            Console.WriteLine($"Critical exception caught: {ex.Message}");
            var seed = _gameState.Seed;
            var ticks = _gameState.CurrentTick;
            await _cloudIntegrationService.Announce(CloudCallbackType.Failed, e: ex, seed: seed, ticks: ticks);
            await _hubConnection.SendAsync("GameFailed", ex.Message, seed, ticks);
        }
        private void BotConnectionTimeout(object? sender, ElapsedEventArgs e)
        {
            if (_gameState.BotManager.GetAllBotStates().Count < _gameSettings.NumberOfPlayers)
            {
                var failReason = $"Only {_gameState.BotManager.GetAllBotStates().Count} out of {_gameSettings.NumberOfPlayers} bots connected in time, runner is shutting down.";
                Log.Error(failReason);

                _cloudIntegrationService.Announce(CloudCallbackType.Failed, new Exception(failReason)).Wait();
                _hubConnection.SendAsync("GameFailed", failReason, _gameState.Seed, 0).Wait();
            }
        }
    }

}