namespace Domain.Models
{
    public class AppSettings
    {
        private static string appEnvironment
        {
            get
            {
                return Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Development";
            }
        }
        public string? ApiUrl
        {
            get
            {
                return IsCloud ? Environment.GetEnvironmentVariable("API_URL") : null;
            }
        }
        public string? ApiKey
        {
            get
            {
                return IsCloud ? Environment.GetEnvironmentVariable("API_KEY") : null;
            }
        }
        public string? MatchId
        {
            get
            {
                return IsCloud ? Environment.GetEnvironmentVariable("MATCH_ID") : null;
            }
        }
        public bool IsLocal => appEnvironment.Equals("Development", StringComparison.InvariantCultureIgnoreCase);
        public bool IsProduction => appEnvironment.Equals("Production", StringComparison.InvariantCultureIgnoreCase);
        public bool IsCloud => !IsLocal;
    }
}
