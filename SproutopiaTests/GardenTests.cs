using NetTopologySuite.Geometries;
using Sproutopia.Models;

namespace SproutopiaTests;

[TestFixture]
public class GardenTests
{
    private Garden garden;

    [SetUp]
    public void Setup()
    {
        Geometry claimedLand = new Point(0, 0);
        garden = new(new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"), claimedLand);
    }

    [Test]
    public void Test_GardenTrailLength()
    {
        Geometry claimedLand = new Point(0, 0);
        Garden garden = new(new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"), claimedLand);

        Coordinate[] trailCoordinatesSingleLine = {
            new(0, 0),
            new(0, 10),
        };
        garden.Trail = new LineString(trailCoordinatesSingleLine);
        Assert.That(garden.TrailLength, Is.EqualTo(10));

        Coordinate[] trailCoordinatesJaggedLine =
        {
            new(0, 0),
            new(0, 10),
            new(10, 10),
        };

        garden.Trail = new LineString(trailCoordinatesJaggedLine);
        Assert.That(garden.TrailLength, Is.EqualTo(20));
    }

    [Test]
    public void Test_GardenHasTrail()
    {
        Geometry claimedLand = new Point(0, 0);
        Garden garden = new(new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"), claimedLand);
        Coordinate[] trailCoordinates =
        {
            new(0, 0),
            new(0, 10),
        };
        garden.Trail = new LineString(trailCoordinates);
        Assert.That(garden.HasTrail, Is.True);

        garden.Trail = null;
        Assert.That(garden.HasTrail, Is.False);
    }

    [Test]
    public void Test_GardenArea()
    {
        Coordinate[] claimedLandCoordinates =
        {
            new(0, 0),
            new(11, 0),
            new(11, 11),
            new(0, 11),
            new(0, 0),
        };

        Geometry claimedLand = new Polygon(new LinearRing(claimedLandCoordinates));
        Garden garden = new Garden(new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"), claimedLand);

        Assert.That(garden.ClaimedArea, Is.EqualTo(121));
    }

    [Test]
    [TestCase(5, 5, true, TestName = "Point in area")]
    [TestCase(15, 5, false, TestName = "Point outside area")]
    [TestCase(0, 5, true, TestName = "Point on edge")]
    [TestCase(0, 0, true, TestName = "Point in corner")]
    public void Test_IsPointInClaimedLand(int x, int y, bool expected)
    {
        // Arrange
        Coordinate[] claimedLandCoordinates =
        {
            new(0, 0),
            new(10, 0),
            new(10, 10),
            new(0, 10),
            new(0, 0),
        };
        Geometry claimedLand = new Polygon(new LinearRing(claimedLandCoordinates));
        Garden garden = new Garden(new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"), claimedLand);

        // Act
        bool result = garden.IsCellInClaimedLand(new CellCoordinate(x, y));

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase(0, 0, true, TestName = "Point on start of trail")]
    [TestCase(10, 10, true, TestName = "Point on end of trail")]
    [TestCase(0, 10, true, TestName = "Point on vertex of trail")]
    [TestCase(0, 5, true, TestName = "Point in middle straight section")]
    [TestCase(11, 10, false, TestName = "Point ahead of trail")]
    [TestCase(5, 5, false, TestName = "Point off trail")]
    public void Test_IsPointOnTrail(int x, int y, bool expected)
    {
        // Arrange
        Geometry claimedLand = new Point(0, 0);
        Garden garden = new(new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"), claimedLand);
        Coordinate[] trailCoordinates = {
            new(0, 0),
            new(0, 10),
            new(10, 10),
        };
        garden.Trail = new LineString(trailCoordinates);

        // Act
        bool result = garden.IsPointOnTrail(new CellCoordinate(x, y));

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Test_GardenAddToTrail()
    {
        Assert.That(garden.TrailLength, Is.EqualTo(0));

        bool result;
        result = garden.AddToTrail(new CellCoordinate(0, 0));
        Assert.That(result, Is.True);
        Assert.That(garden.TrailLength, Is.EqualTo(0));
        Assert.That(garden.Trail, Is.Not.Null);

        result = garden.AddToTrail(new CellCoordinate(0, 0));
        Assert.That(result, Is.True);
        Assert.That(garden.TrailLength, Is.EqualTo(0));

        result = garden.AddToTrail(new CellCoordinate(0, 1));
        Assert.That(result, Is.True);
        Assert.That(garden.TrailLength, Is.EqualTo(1));

        result = garden.AddToTrail(new CellCoordinate(1, 1));
        Assert.That(result, Is.True);
        Assert.That(garden.TrailLength, Is.EqualTo(2));

        result = garden.AddToTrail(new CellCoordinate(1, 10));
        Assert.That(result, Is.True);
        Assert.That(garden.TrailLength, Is.EqualTo(11));

        result = garden.AddToTrail(new CellCoordinate(1, 10));
        Assert.That(result, Is.True);
        Assert.That(garden.TrailLength, Is.EqualTo(11));
    }

    [Test]
    public void Test_GardenIntersectOwnTrail()
    {
        bool[] results = new bool[5];
        results[0] = garden.AddToTrail(new CellCoordinate(0, 0));
        results[1] = garden.AddToTrail(new CellCoordinate(0, 1));
        results[2] = garden.AddToTrail(new CellCoordinate(1, 1));
        results[3] = garden.AddToTrail(new CellCoordinate(1, 0));
        results[4] = garden.AddToTrail(new CellCoordinate(0, 0));
        Assert.That(garden.TrailLength, Is.EqualTo(3));
        Assert.That(garden.Trail?.IsClosed, Is.False);
        Assert.That(results, Is.EqualTo(new[]
        {
            true,
            true,
            true,
            true,
            false,
        }));
    }

    [Test]
    public void Test_PruneClaimedLand()
    {
        // Arrange
        var garden = new Garden(
            new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"),
            new Polygon(new LinearRing(new Coordinate[] {
                new(0, 0),
                new(0, 10),
                new(10, 10),
                new(10, 0),
                new(0, 0),
        })));

        // Act
        garden.PruneClaimedLand();

        // Assert
        Assert.That(garden.ClaimedArea, Is.EqualTo(0));
    }
}