namespace Sproutopia
{
    public sealed class RunnerCommands
    {
        public static readonly RunnerCommands Registered = new("Registered");
        public static readonly RunnerCommands Disconnect = new("Disconnect");
        public static readonly RunnerCommands ReceiveBotState = new("ReceiveBotState");
        public static readonly RunnerCommands ReceiveGameComplete = new("ReceiveGameComplete");
        public static readonly RunnerCommands EndGame = new("EndGame");


        private readonly string Value;

        private RunnerCommands(string value) { Value = value; }

        public static implicit operator string(RunnerCommands value) => value.Value;
    }
}
