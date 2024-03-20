using SproutReferenceBot.Enums;
using SproutReferenceBot.Models;

namespace SproutReferenceBot.Services;
  class BotService
  {
    private Guid BotId;
    private BotStateDTO LastKnownState;
    
    private static readonly int SquareSize = 5;

    private static readonly BotAction[] ActionOrder = new[]
    {
      BotAction.Up,
      BotAction.Right,
      BotAction.Down,
      BotAction.Left,
    };

    private int CurrentAction = 0;
    private int StepsTaken = 0;

    public BotCommand ProcessState(BotStateDTO BotState)
    {
      if (StepsTaken++ >= SquareSize)
      {
        CurrentAction = (CurrentAction + 1) % ActionOrder.Length;
        StepsTaken = 0;
      }

      var ActionToTake = ActionOrder[CurrentAction];
      
      return new BotCommand
      {
        BotId = BotId,
        Action = ActionToTake,
      };
    }

    public void SetBotId(Guid NewBotId)
    {
      BotId = NewBotId;
    }
  }
