using Domain;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Logger.Utilities;
using Microsoft.AspNetCore.SignalR;
using Runner.Services;
using Serilog;

namespace Sproutopia
{
    public class RunnerHub : Hub
    {
        private readonly IEngine _engine;
        private readonly ICloudIntegrationService _cloudIntegrationService;

        public RunnerHub(IEngine engine,
          ICloudIntegrationService cloudIntegrationService)
        {
            _engine = engine;
            _cloudIntegrationService = cloudIntegrationService;
        }

        #region Runner endpoints

        /// <summary>
        ///     New client connected
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            try
            {
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnConnectedAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// De-regsiter a bot on disconnect
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Log.Error(exception, exception?.Message);
            //TODO: Implement any game specific logic to remove a bot
            return base.OnDisconnectedAsync(exception);
        }

        #endregion

        #region Game Engine endpoints

        /// <summary>
        /// When the game is complete.
        /// </summary>
        /// <returns></returns>
        public async Task GameComplete(int? seed, int? ticks)
        {
            Console.WriteLine("Announcing Game Completed");
            Log.Information("Game Complete");


            await Clients.All.SendAsync(RunnerCommands.ReceiveGameComplete);
            await _cloudIntegrationService.Announce(CloudCallbackType.Finished, seed: seed, ticks: ticks);

            await S3.UploadLogs();
            await _cloudIntegrationService.Announce(CloudCallbackType.LoggingComplete, null, null);
        }

        /// <summary>
        /// When a critical error prevents the game from finishing.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="seed">The game seed.</param>
        /// <param name="ticks">The number of ticks.</param>
        public async Task GameFailed(String message, int? seed, int? ticks)
        {
            Console.WriteLine("Announcing Game Failed");
            Log.Error($"Game failed with exception: {message}");

            await Clients.All.SendAsync(RunnerCommands.Disconnect, "A critical error prevented the game from completing.");



            await S3.UploadLogs();
            await _cloudIntegrationService.Announce(CloudCallbackType.LoggingComplete);
        }

        #endregion

        #region Bot endpoints
        /// <summary>
        /// Allows bot to register for game with given token
        /// </summary>
        /// <param name="nickName"></param>
        /// <returns></returns>
        public async Task Register(Guid token, string nickName)
        {
            try
            {
                _engine.RegisterBot(token, nickName, Context.ConnectionId);
                Log.Debug($"{token}: REGISTERED Bot With Nickname {nickName}");

                await Clients.Caller.SendAsync(RunnerCommands.Registered, token);
                await CheckStartConditions();
            }
            catch (BotCapacityReachedException ex)
            {
                Log.Information(ex.Message);
                await Clients.Caller.SendAsync(RunnerCommands.Disconnect, ex.Message);
                return;
            }

        }

        /// <summary>
        ///     Allow bots to send actions to Engine
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task SendPlayerCommand(BotCommand command)
        {
            if (_engine.IsBotRegistered(command.BotId))
            {
                Log.Information($"{command.BotId.ToString().Take(4)}: RECIEVED player command {(BotAction)command.Action}");
                await _engine.AddCommandToBotQueue(command);
            }
        }
        #endregion

        #region Private methods

        private async Task CheckStartConditions()
        {
            if (_engine.IsStartConditionsMet())
            {
                Log.Information("Game Starting!");

                await _cloudIntegrationService.Announce(CloudCallbackType.Started);

                await _engine.StartGame();
            }
            else
            {
                Log.Information("Waiting for next bot to connect...");
            }
        }
        #endregion
    }
}
