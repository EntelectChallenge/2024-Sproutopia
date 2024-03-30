using Domain;
using Domain.Models;
using Microsoft.Extensions.Options;
using NUnit.Framework.Internal;
using Sproutopia.Enums;
using Sproutopia.Managers;
using Sproutopia.Models;
using SproutopiaTests.Mocks;
using System.Text;

namespace SproutopiaTests;

/// <summary>
/// These tests make use of a mocked GardenManager which allows the test case to specify what response should be received when
/// PerformAction() is called. This means it's not necessary to set up an entire GardenManager with actual powerups, bots and
/// territories, because that is not what is being tested in this file.
/// </summary>
[TestFixture]
public class GameStateTests
{
    private BotManager _botManager;
    private IOptions<SproutopiaGameSettings> _gameSettings;

    [SetUp]
    public void Setup()
    {
        _gameSettings = Options.Create(new SproutopiaGameSettings() { Seed = 123 });
        _botManager = new BotManager(_gameSettings);
    }

    /// <summary>
    /// Test that bot positions in BotManager are updated correctly when response is received from GardenManager.PerformAction()
    /// The test initiates a bot with a position of 10,10 and tests the position after execution of IssueCommand which may or may
    /// not have updated the bot position depending on what GardenManager.PerformAction returned.
    /// </summary>
    /// <param name="inx">X coordinate of new position received from PerformAction</param>
    /// <param name="iny">Y coordinate of new position received from PerformAction</param>
    /// <param name="expx">expected X coordinate of bot after update</param>
    /// <param name="expy">expected Y coordinate of bot after update</param>
    [Test]
    [TestCase(null, null, 10, 10)]
    [TestCase(5, 5, 5, 5)]
    public void Test_IssueCommand_Can_Update_Bot_Position(int? inx, int? iny, int expx, int expy)
    {
        // Arrange
        var botId = Guid.NewGuid();
        _botManager.AddBot(new BotState(botId, new (10, 10)));
        var gameState = new GameState(
            gameSettings: _gameSettings,
            botManager: _botManager,
            gardenManager: new MockGardenManager(new BotResponse
            {
                NewPosition = (inx == null) ? null : new((int)inx, (int)iny),
                Alive = true,
            }));

        // Act
        gameState.IssueCommand(new SproutBotCommand(botId, BotAction.Up));

        // Assert
        Assert.That(_botManager.GetBotState(botId).Position, Is.EqualTo(new CellCoordinate(expx, expy)));
    }

    /// <summary>
    /// Tests that BotState in BotManager is updated correctly to reflect the bot's active powerup and that the powerup is removed
    /// from world map when a bot excavates a powerup.
    /// </summary>
    /// <param name="excavate">Boolean value determining whether the command issued for a bot results in the excavation of a powerup</param>
    /// <param name="expectedCount">Expected number of powerups on the world map after issued command has ben processed</param>
    /// <param name="expectedPowerUpType">Expected active power up on bot after issued command has been processed</param>
    [Test]
    [TestCase(false, 1, null)]
    [TestCase(true, 0, PowerUpType.TerritoryImmunity)]
    public void Test_IssueCommand_Can_Excavate_PowerUp(bool excavate, int expectedCount, PowerUpType? expectedPowerUpType)
    {
        // Arrange
        var botId = Guid.NewGuid();

        _botManager.AddBot(new BotState(botId, new(10, 10)));
        PowerUp powerUp = new(PowerUpType.TerritoryImmunity, new(5, 5));
        var powerUpId = powerUp.Id;
        List<PowerUp> powerUps = [];
        powerUps.Add(powerUp);
        var pu = excavate ? powerUp : null;

        var gardenManager = new MockGardenManager(new BotResponse
        {
            Alive = true,
            PowerUpExcavated = pu,
        });
        gardenManager.AddPowerUp(powerUpId);

        var gameState = new GameState(
            gameSettings: _gameSettings,
            botManager: _botManager,
            gardenManager: gardenManager);

        // Act
        gameState.IssueCommand(new SproutBotCommand(botId, BotAction.Up));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_botManager.GetBotState(botId).GetActivePowerUp(), Is.EqualTo(expectedPowerUpType));
            Assert.That(gardenManager.PowerUpCount(), Is.EqualTo(expectedCount));
        });
    }
}
