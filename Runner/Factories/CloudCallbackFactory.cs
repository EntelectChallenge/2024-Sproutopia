using Domain.Enums;
using Domain.Models;

namespace Runner.Factories
{
    public static class CloudCallbackFactory
    {
        public static CloudCallback Build(string matchId, CloudCallbackType callbackType, Exception? e, int? seed, int? ticks)
        {
            return callbackType switch
            {
                CloudCallbackType.Initializing => new CloudCallback
                {
                    MatchId = matchId,
                    MatchStatus = "initializing",
                    MatchStatusReason = "Startup"
                },
                CloudCallbackType.Ready => new CloudCallback
                {
                    MatchId = matchId,
                    MatchStatus = "ready",
                    MatchStatusReason = $"All Components connected and ready for bots. Waiting for bots to connect."
                },
                CloudCallbackType.Started => new CloudCallback
                {
                    MatchId = matchId,
                    MatchStatus = "started",
                    MatchStatusReason = $"Match has started with bots",
                    Players = new List<CloudPlayer>(),
                },
                CloudCallbackType.Failed => new CloudCallback
                {
                    MatchId = matchId,
                    MatchStatus = "failed",
                    MatchStatusReason = e?.Message ?? "",
                    Players = new List<CloudPlayer>()
                },
                CloudCallbackType.Finished => new CloudCallback
                {
                    MatchId = matchId,
                    MatchStatus = "finished",
                    MatchStatusReason = "Game Complete.",
                    Seed = seed.ToString() ?? "",
                    Ticks = ticks.ToString() ?? "",
                    Players = new List<CloudPlayer>(),
                },
                CloudCallbackType.LoggingComplete => new CloudCallback
                {
                    MatchId = matchId,
                    MatchStatus = "logging_complete",
                    MatchStatusReason = "Game Complete. Logging Complete.",
                    Seed = seed.ToString() ?? "",
                    Ticks = ticks.ToString() ?? "",
                    Players = new List<CloudPlayer>(),
                },
                _ => throw new ArgumentOutOfRangeException(nameof(callbackType), callbackType, "Unknown Cloud Callback Type")
            };
        }
    }
}
