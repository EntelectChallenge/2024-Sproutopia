using Domain.Models;
using Logger;

namespace Domain.Interfaces
{
    public interface IEngine
    {
        public IStreamingFileLogger StateLogger { get; }
        public IStreamingFileLogger GameCompleteLogger { get; }

        /// <summary>
        /// Function to Register the bot
        /// </summary>
        /// <param name="Token">Unique identifier</param>
        /// <param name="NickName">Nickname for bot</param>
        /// <param name="ConnectionId">Connection Id</param>
        void RegisterBot(Guid Token, string NickName, string ConnectionId);

        /// <summary>
        /// Determines if the bot is registered 
        /// </summary>
        /// <param name="botId">Unique identifier for the Bot</param>
        /// <returns>True or false depicting whether the bot is registered or not</returns>
        bool IsBotRegistered(Guid botId);

        /// <summary>
        /// Determines if the start conditions are met 
        /// </summary>
        /// <returns>True or false depicting whether the start conditions are met</returns>
        bool IsStartConditionsMet();

        /// <summary>
        /// Checks the bot guid and adds the command to the correct bot queue
        /// </summary>
        /// <param name="botCommand"> <see cref="BotCommand"/> </param>
        Task AddCommandToBotQueue(BotCommand botCommand);

        /// <summary>
        /// Returns static game information on request
        /// </summary>
        /// <param name="botId">Id of requesting bot</param>
        Task RequestGameInfo(Guid botId);

        /// <summary>
        /// Sets up hub connection and begins timer 
        /// </summary>
        Task StartGame();
    }
}
