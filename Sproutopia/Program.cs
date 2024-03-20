using Domain.Enums;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Runner.Services;
using Serilog;
using Serilog.Extensions.Logging;
using Sproutopia.Managers;
using Sproutopia.Models;

namespace Sproutopia
{
    public static class Program
    {
        private static IConfigurationRoot? configuration;

        public static async Task Main(string[] args)
        {
            string environment;
#if DEBUG
            environment = "Development";
#elif RELEASE
            environment = "Production";
#endif

            configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "{Timestamp:o} {Level} {SourceContext} - {Message}{NewLine}{Expression}")
                .MinimumLevel.Debug()
                .CreateLogger();

            AppSettings appSettings = new();
            ILogger<CloudIntegrationService> cloudLog = new SerilogLoggerFactory(Log.Logger).CreateLogger<CloudIntegrationService>();
            CloudIntegrationService cloudIntegrationService = new(appSettings, cloudLog);

            try
            {
                IHost host = Host.CreateDefaultBuilder(args)
                    .ConfigureServices((context, services) =>
                    {
                        services.AddSingleton<IConfiguration>(provider => configuration);
                        services.AddSingleton<AppSettings>();
                        services.Configure<SproutopiaGameSettings>(configuration.GetSection("GameSettings"));
                        services.AddSingleton<GlobalSeededRandomizer>();
                        services.AddSingleton<ICloudIntegrationService>(cloudIntegrationService);
                        services.AddSingleton<ITickManager, TickManager>();
                        services.AddSingleton<IEngine, SproutopiaEngine>();
                        services.AddSingleton<GameState>();
                        services.AddSingleton<IBotManager, BotManager>();
                        services.AddSingleton<IGardenManager, GardenManager>();
                        services.AddSingleton<RunnerHub>();
                        services.AddSignalR(options =>
                        {
                            //Uncomment for detailed error log for SignalR
                            options.EnableDetailedErrors = true;
                            options.MaximumReceiveMessageSize = 40000000;
                            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);

                            //  options.
                        });//.AddMessagePack : used to compress JSON objects does not seem to be avaiable in version 8.0.0;
                        services.AddHostedService<EngineWorker>();
#if DEBUG
                        services.AddSingleton<VisualiserHub>();
#endif

                    }).ConfigureWebHostDefaults(webBuilder =>
                    {
                        //TODO use the confgiruration
                        webBuilder.UseUrls("http://*:5000");
                        webBuilder.Configure(app =>
                        {
                            app.UseRouting();

                            app.UseEndpoints(endpoints =>
                            {
                                endpoints.MapHub<RunnerHub>("/runnerhub");
#if DEBUG
                                endpoints.MapHub<VisualiserHub>("/visualiserhub");
#endif
                            });
                        });
                    })
                    .UseSerilog()
                    .Build();


                var signalRConfig = configuration.GetSection("SignalR").GetChildren().ToDictionary(x => x.Key, x => x.Value);

                var connection = new HubConnectionBuilder().WithUrl(signalRConfig["RunnerURL"]).Build();

                Console.WriteLine($"SignalR Config{signalRConfig["RunnerURL"]}");

                connection.KeepAliveInterval = TimeSpan.FromSeconds(1000);
                connection.ServerTimeout = TimeSpan.FromSeconds(1000);

                ((SproutopiaEngine)host.Services.GetRequiredService<IEngine>()).SetHubConnection(ref connection);
                await cloudIntegrationService.Announce(CloudCallbackType.Initializing);

                Log.Information("Running Sproutopia");
                host.Run();
            }
            catch (Exception ex)
            {
                await cloudIntegrationService.Announce(CloudCallbackType.Failed, ex);
            }
        }
    }
}