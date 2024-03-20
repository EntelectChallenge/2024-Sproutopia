using Microsoft.AspNetCore.Components.RenderTree;
using NetTopologySuite.Geometries;
using Sproutopia.Models;

namespace SproutopiaTests;

[TestFixture]
public class WeedTests
{
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
}