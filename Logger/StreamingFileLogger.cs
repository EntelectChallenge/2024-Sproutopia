using Serilog;

namespace Logger
{
    public class StreamingFileLogger : IStreamingFileLogger
    {
        private readonly string LOG_PATH;

        private readonly StreamWriter stream;

        public StreamingFileLogger(string fileName)
        {
            var LOG_DIRECTORY = Path.Combine(AppContext.BaseDirectory[..AppContext.BaseDirectory.IndexOf("Sproutopia")], "Logs");
            Environment.SetEnvironmentVariable("LOG_DIR", LOG_DIRECTORY);
            LOG_PATH = Path.Combine(LOG_DIRECTORY, $"{fileName}.json.gz");

            Serilog.Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .WriteTo.File(@"f:\log\log.txt")
                .CreateLogger();

            //  FileStream fileStream = new(LOG_PATH, FileMode.OpenOrCreate);
            //  GZipStream gZipStream = new(fileStream, CompressionMode.Compress);
            // stream = new(gZipStream);
            //  stream.Write("[");
        }

        public void Log(object state)
        {
            Serilog.Log.Information("{@state}", state);
        }
        public async Task Close()
        {
            await stream.WriteLineAsync("]");
            stream.Close();
            stream.Dispose();
        }

        Task IStreamingFileLogger.Log(object state)
        {
            throw new NotImplementedException();
        }
    }
}
