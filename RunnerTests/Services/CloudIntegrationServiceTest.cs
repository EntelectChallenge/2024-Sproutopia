using Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Runner.Services;

namespace RunnerTests.Services
{
    [TestFixture]
    public class CloudIntegrationServiceTest
    {
        CloudIntegrationService serviceUnderTest;

        [SetUp]
        public void SetUp()
        {
            AppSettings testAppSettings = new();
            ILogger<CloudIntegrationService> logger = new NullLogger<CloudIntegrationService>();
            serviceUnderTest = new CloudIntegrationService(testAppSettings, logger);
        }

        [Test]
        public void WhenAddingBlankPlayer_PlayerShouldBeInList_WithInitialData()
        {
            // Arrange

            // Act
            serviceUnderTest.AddPlayer(playerId: "0");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(serviceUnderTest.Players.Count() > 0, Is.True);
                Assert.That(serviceUnderTest.Players.First().GamePlayerId, Is.EqualTo("0"));
            });
        }

        [Test]
        public void WhenUpdatingPlater_IfPlayerNotInList_DoNothing()
        {
            // Arrange
            serviceUnderTest.AddPlayer(playerId: "0");

            // Act
            serviceUnderTest.UpdatePlayer("1", 1, 1, 1);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(serviceUnderTest.Players.First(p => p.GamePlayerId.Equals("0")).FinalScore, Is.EqualTo(0));
                Assert.That(serviceUnderTest.Players.First(p => p.GamePlayerId.Equals("0")).Placement, Is.EqualTo(0));
            });
        }

        [Test]
        public void WhenUpdatingPlater_IfPlayerInList_UpdatePlayer()
        {
            // Arrange
            serviceUnderTest.AddPlayer(playerId: "123");

            // Act
            serviceUnderTest.UpdatePlayer("123", 1, 1, 1);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(serviceUnderTest.Players.First().FinalScore, Is.EqualTo(1));
                Assert.That(serviceUnderTest.Players.First().Placement, Is.EqualTo(1));
            });
        }
    }
}
