using Domain.Enums;
using Domain.Models;
using dummybot;
using Microsoft.AspNetCore.SignalR.Client;
using Runner.DTOs;
using Serilog;
using Sproutopia;
using Sproutopia.Models;

DummyBot dummy = new DummyBot();
dummy.Main();

namespace dummybot
{
    class DummyBot
    {
        HubConnection connection;
        Guid botId;
        bool isConnected = false;
        string botNickname;

        public DummyBot()
        {
            connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/runnerhub")
                .Build();

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(/*outputTemplate: "{Timestamp:o} {Level} {SourceContext} - {Message}{NewLine}{Expression}"*/)
                .MinimumLevel.Debug()
                .CreateLogger();
        }

        public void Main()
        {
            Console.WriteLine("Enter bot nickname:");
            botNickname = Console.ReadLine();

            connection.On<Guid>(RunnerCommands.Registered, guid =>
            {
                botId = guid;
            });
            connection.On<BotStateDTO>(RunnerCommands.ReceiveBotState, botState =>
            {
                Log.Information("Received:");
                Log.Information("{@botState}", botState);
            });
            connection.On<GameInfoDTO>(RunnerCommands.ReceiveGameInformation, gameInfo =>
            {
                Log.Information("Game Information:");
                Log.Information("{@gameInfo}", gameInfo);
            });

            connection.On<Guid>(RunnerCommands.EndGame, guid =>
            {
                isConnected = false;
            });

            do
            {
                if (!isConnected)
                {
                    connection.StartAsync().Wait();
                    connection.SendAsync("Register", Guid.NewGuid(), botNickname).Wait();

                    isConnected = true;
                }

                Console.WriteLine("Enter Command:");
                var input = Console.ReadKey();

                switch (input.Key)
                {
                    case ConsoleKey.W:
                        SendCommand(BotAction.Up);
                        break;
                    case ConsoleKey.A:
                        SendCommand(BotAction.Left);
                        break;
                    case ConsoleKey.S:
                        SendCommand(BotAction.Down);
                        break;
                    case ConsoleKey.D:
                        SendCommand(BotAction.Right);
                        break;
                    case ConsoleKey.I:
                        GetGameInfo();
                        break;
                    case ConsoleKey.Q:
                        Console.Clear();
                        isConnected = false;
                        break;
                    default:
                        Console.WriteLine("Please enter valid command : W/A/S/D");
                        break;
                }

            } while (isConnected);

            //End Game
            connection.StopAsync();
            //TODO: add some graceful errors 
            //connection.Closed();
        }

        private void SendCommand(BotAction action) =>
            connection.SendAsync("SendPlayerCommand", new SproutBotCommand(botId, action));

        private void GetGameInfo() =>
            connection.SendAsync("GetGameInfo", botId);
    }
}