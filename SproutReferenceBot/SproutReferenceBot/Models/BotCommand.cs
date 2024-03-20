using SproutReferenceBot.Enums;

namespace SproutReferenceBot.Models;

public class BotCommand
{
    public Guid BotId { get; set; }
    public BotAction Action { get; set; }
}