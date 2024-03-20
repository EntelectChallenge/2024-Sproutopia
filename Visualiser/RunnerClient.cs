using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Visualiser
{
    public static class RunnerClient
    {
        private static IConfigurationRoot Configuration;
        public static HubConnection connection { get; private set; }


        public static async Task ConnectToRunner()
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder().AddJsonFile(
                "appsettings.json",
                optional: false
            );

            Configuration = builder.Build();
            var ip = Configuration.GetSection("SignalR:RunnerIP").Value;

            var botNickname =
                Environment.GetEnvironmentVariable("BOT_NICKNAME")
                ?? Configuration.GetSection("SignalR:BotNickname").Value;

            //         var token =
            //              Environment.GetEnvironmentVariable("Token") ??
            //              Environment.GetEnvironmentVariable("REGISTRATION_TOKEN");

            var port = Configuration.GetSection("SignalR:RunnerPort");

            var url = ip + ":" + port.Value + "/visualiserhub";

            connection = new HubConnectionBuilder()
                .WithUrl($"{url}")
                .ConfigureLogging(logging =>
                {
                    //  logging.SetMinimumLevel(LogLevel.Debug);
                })
                .WithAutomaticReconnect()
                .Build();

            await connection.StartAsync();

        }

    }
}
