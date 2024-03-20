using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Serilog;
using Sproutopia.Managers;

namespace Sproutopia
{
    public class VisualiserHub : Hub
    {
        private readonly ILogger<RunnerHub> _logger;
        private readonly ITickManager _tickManager;
        public VisualiserHub(
            ILogger<RunnerHub> logger,
            ITickManager tickManager)
        {
            _logger = logger;
            _tickManager = tickManager;
        }


        /// <summary>
        ///     New client connected
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            try
            {
                _logger.LogDebug("New Connection");
                await base.OnConnectedAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnConnectedAsync: {ex.Message}");
                throw;
            }
        }
        #region Visualiser endpoints

        public async Task StepIntoGame()
        {
            _logger.LogInformation("Stepping into Game ......");
            try
            {
                _tickManager.Step();
            }
            catch (Exception ex)
            {
                Log.Information(ex.Message);
                await Clients.Caller.SendAsync(VisualiserCommands.Disconnect, ex.Message);
            }
        }

        public async Task StopGame()
        {
            _logger.LogInformation("Stopping Game ......");
            try
            {
                _logger.LogInformation("Game stopped......");
            }
            catch (Exception ex)
            {
                Log.Information(ex.Message);
                await Clients.Caller.SendAsync(VisualiserCommands.Disconnect, ex.Message);
            }
        }
        public async Task ContinueGame()
        {
            _logger.LogInformation("Continue Game ......");
            try
            {
                _tickManager.Continue();
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                await Clients.Caller.SendAsync(VisualiserCommands.Disconnect, ex.Message);
            }
        }

        public async Task PauseGame()
        {
            Log.Information("Pausing Game ......");
            try
            {
                _tickManager.Pause();
            }
            catch (Exception ex)
            {
                Log.Information(ex.Message);
                await Clients.Caller.SendAsync(VisualiserCommands.Disconnect, ex.Message);
            }
        }


        #endregion
    }
}
