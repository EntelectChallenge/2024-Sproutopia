using Domain.Enums;
using Domain.Models;
using Microsoft.Extensions.Options;
using NUnit.Framework.Internal;
using Sproutopia.Enums;
using Sproutopia.Managers;
using Sproutopia.Models;
using Sproutopia.Utilities;
using SproutopiaTests.Mocks;

namespace SproutopiaTests;

[TestFixture]
public class GameStateDtoTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test_LeaderBoardByName()
    {
        // Arrange
        var botSnapshots = new List<BotSnapshot>
        {
            new BotSnapshot(0, Guid.Parse("6d8b59ba-200f-49e2-892b-076c609a51da"), "Deefigliano", BotAction.IDLE, BotAction.IDLE, PowerUpType.NONE, SuperPowerUpType.NONE, 0, 0),
            new BotSnapshot(0, Guid.Parse("b5129a3b-0fd5-460e-ac72-53c28dd2df0e"), "Jenatelli", BotAction.IDLE, BotAction.IDLE, PowerUpType.NONE, SuperPowerUpType.NONE, 1, 1),
            new BotSnapshot(0, Guid.Parse("025d8666-2184-43fe-bae8-d27412b3ad67"), "Natashatori", BotAction.IDLE, BotAction.IDLE, PowerUpType.NONE, SuperPowerUpType.NONE, 2, 2),
            new BotSnapshot(0, Guid.Parse("7879764c-b3bc-49a7-84e4-d9aa58bbb9dc"), "Creolingus", BotAction.IDLE, BotAction.IDLE, PowerUpType.NONE, SuperPowerUpType.NONE, 3, 3)
        };

        var leaderBoard = new List<(Guid botId, int percentage)>
        {
            (Guid.Parse("b5129a3b-0fd5-460e-ac72-53c28dd2df0e"), 53),
            (Guid.Parse("025d8666-2184-43fe-bae8-d27412b3ad67"), 21),
            (Guid.Parse("6d8b59ba-200f-49e2-892b-076c609a51da"), 15),
            (Guid.Parse("7879764c-b3bc-49a7-84e4-d9aa58bbb9dc"), 11)
        };

        var gameStateDto = new GameStateDto(
            timeStamp: DateTime.UtcNow,
            currentTick: 1,
            botSnapshots: botSnapshots,
            territory: Helpers.CreateJaggedArray<int[][]>(10, 10),
            trails: Helpers.CreateJaggedArray<int[][]>(10, 10),
            leaderBoard: leaderBoard,
            powerUps: null,
            weeds: null
            );

        // Act
        var leaderBoardByName = gameStateDto.LeaderBoardByName;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(leaderBoardByName[0], Is.EqualTo(("Jenatelli", 53)));
            Assert.That(leaderBoardByName[1], Is.EqualTo(("Natashatori", 21)));
            Assert.That(leaderBoardByName[2], Is.EqualTo(("Deefigliano", 15)));
            Assert.That(leaderBoardByName[3], Is.EqualTo(("Creolingus", 11)));
        });
    }
}
