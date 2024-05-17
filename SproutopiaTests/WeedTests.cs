using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Options;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NUnit.Framework.Internal;
using Sproutopia.Managers;
using Sproutopia.Models;
using System.Drawing;

namespace SproutopiaTests;

[TestFixture]
public class WeedTests
{
    private BotManager _botManager;
    private GlobalSeededRandomizer _randomizer;

    [SetUp]
    public void Setup()
    {
        var settings = Options.Create(new SproutopiaGameSettings() { Seed = 123 });
        _botManager = new BotManager(settings);
        _randomizer = new GlobalSeededRandomizer(settings);
    }

    [Test]
    [TestCase(1, TestName = "Unit Size")]
    [TestCase(10, TestName = "Large Size")]
    public void Test_OvergrownLand(int size)
    {
        // Arrange
        var weed = new Weed(Guid.NewGuid(), new CellCoordinate(10, 10), Sproutopia.Enums.SuperPowerUpType.TrailProtection, 0);

        // Act
        for (int i = 1; i < size; i++)
        {
            weed.CoveredCells.Add(new CellCoordinate(10, 10+i));
        }

        // Assert
        Assert.That((int)weed.OvergrownLand.Area, Is.EqualTo(size));
    }

    [Test]
    public void Test_WeedGrowth()
    {
        // Arrange
        var gardenManager = new GardenManager(13, 13, [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()], _botManager, _randomizer);
        gardenManager.AddWeed(1);
        var weed = gardenManager.GetWeeds().First();

        // Act
        double growth = 1;
        double lastArea = 0;
        while (growth > 0)
        {
            gardenManager.GrowWeed(weed.Id);
            growth = weed.OvergrownLand.Area - lastArea;
            lastArea = weed.OvergrownLand.Area;
        }

        // Assert
        Assert.That((int)weed.OvergrownLand.Area, Is.EqualTo(117)); // 117 = 13x13 grid, minus cells touching the border, minus 4 spawn points
    }
}