using Domain.Enums;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Runner.Factories;
using System.Net.Http.Formatting;

namespace Runner.Services
{
    public class CloudIntegrationService : ICloudIntegrationService
    {
        private readonly AppSettings _appSettings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<CloudIntegrationService> _logger;

        private List<CloudPlayer> players;

        public List<CloudPlayer> Players
        {
            get { return players; }
            set { players = value; }
        }

        public CloudIntegrationService(AppSettings appSettings, ILogger<CloudIntegrationService> logger) 
        {
            _appSettings = appSettings;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _appSettings.ApiKey);
            _logger = logger;
            players = new List<CloudPlayer>();
        }

        public async Task Announce(CloudCallbackType callbackType, Exception? e = null, int? seed = null, int? ticks = null)
        {
            CloudCallback cloudPayload = CloudCallbackFactory.Build(_appSettings.MatchId ?? "",  callbackType, e, seed, ticks);
            if (cloudPayload.Players != null)
            {
                cloudPayload.Players = players;
            }
            
            try
            {
                var result = await _httpClient.PostAsync(_appSettings.ApiUrl, cloudPayload, new JsonMediaTypeFormatter());
                if (!result.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Received non-success status code from cloud callback. Code: {statusCode}", result.StatusCode);
                }
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to make cloud callback with error: {message}", ex.Message);
            }
        }

        public void AddPlayer(
            int finalScore = 0,
            string playerId = "",
            int matchPoints = 0,
            int placement = 0,
            string participationId = "",
            string seed = ""
        )
        {
            players.Add(new CloudPlayer()
            {
                FinalScore = finalScore,
                GamePlayerId = playerId,
                MatchPoints = matchPoints,
                Placement = placement,
                PlayerParticipantId = participationId,
                Seed = seed
            });
        }

        public void UpdatePlayer(string playerId, int? finalScore = null, int? matchPoints = null, int? placement = null)
        {
            players.ForEach(player =>
            {
                if (player.GamePlayerId != null && player.GamePlayerId.Equals(playerId, StringComparison.InvariantCultureIgnoreCase))
                {
                    player.FinalScore = finalScore ?? player.FinalScore;
                    player.MatchPoints = matchPoints ?? player.MatchPoints;
                    player.Placement = placement ?? player.Placement;
                }
            });
        }
    }
}
