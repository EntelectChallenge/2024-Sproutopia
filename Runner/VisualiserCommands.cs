namespace Sproutopia
{
    public sealed class VisualiserCommands
    {
        public static readonly VisualiserCommands Registered = new("Registered");
        public static readonly VisualiserCommands Disconnect = new("Disconnect");
        public static readonly VisualiserCommands ReceiveInitialGameState = new("ReceiveInitialGameState");
        public static readonly VisualiserCommands ReceiveUpdatedGameState = new("ReceiveUpdatedGameState");
        public static readonly VisualiserCommands SendDummyString = new("SendDummyString");
        public static readonly VisualiserCommands EndGame = new("EndGame");

        private readonly string Value;

        private VisualiserCommands(string value) { Value = value; }

        public static implicit operator string(VisualiserCommands value) => value.Value;
    }
}
