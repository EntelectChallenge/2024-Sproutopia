namespace SproutReferenceBot.Models;

public class BotStateDTO
{
    public int DirectionState { get; set; }
    public string ElapsedTime { get; set; }
    public int GameTick { get; set; }
    public int PowerUp { get; set; }
    public int SuperPowerUp { get; set; }

    /// <summary>
    /// Represents the current leader board with {Nickname, Territory percentage}   
    /// </summary>
    public Dictionary<Guid, int> LeaderBoard { get; set; }

    public List<(int x, int y)> BotPostions { get; set; }
    public List<(int x, int y, int type)> PowerUps { get; set; }
    public bool[][] Weeds { get; set; }
    public int[][] HeroWindow { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public BotStateDTO(
        int directionState,
        string elapsedTime,
        int gameTick,
        int powerUp,
        int superPowerUp,
        Dictionary<Guid, int> leaderBoard,
        List<(int x, int y)> botPostions,
        List<(int x, int y, int type)> powerUps,
        bool[][] weeds,
        int[][] heroWindow,
        int x,
        int y
    )
    {
        DirectionState = directionState;
        ElapsedTime = elapsedTime;
        GameTick = gameTick;
        PowerUp = powerUp;
        SuperPowerUp = superPowerUp;
        LeaderBoard = leaderBoard;
        BotPostions = botPostions;
        PowerUps = powerUps;
        Weeds = weeds;
        HeroWindow = heroWindow;
        X = x;
        Y = y;
    }
}