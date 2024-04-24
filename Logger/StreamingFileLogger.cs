using Serilog;

namespace Logger
{
    public class StreamingFileLogger : IStreamingFileLogger
    {
        private readonly string LOG_PATH;

        private readonly StreamWriter stream;

        public StreamingFileLogger(string fileName)
        {
            var LOG_DIRECTORY = Environment.GetEnvironmentVariable("LOG_DIR") ?? Path.Combine(AppContext.BaseDirectory[..AppContext.BaseDirectory.IndexOf("Sproutopia")], "Logs");
            Environment.SetEnvironmentVariable("LOG_DIR", LOG_DIRECTORY);
            LOG_PATH = Path.Combine(LOG_DIRECTORY, $"{fileName}.json.gz");

            Serilog.Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .WriteTo.File(LOG_PATH)
                .CreateLogger();

            Serilog.Log.Information("[");
        }

        public void Log(object state)
        {
            Serilog.Log.Information("{@state}", state);
        }

        public async Task Close()
        {
            Serilog.Log.Information("]");
            Serilog.Log.CloseAndFlush();
        }

        Task IStreamingFileLogger.Log(object state)
        {
            throw new NotImplementedException();
        }
    }
}
