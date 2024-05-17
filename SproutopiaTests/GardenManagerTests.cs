using Domain.Enums;
using Domain.Models;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using Sproutopia.Domain;
using Sproutopia.Enums;
using Sproutopia.Managers;
using Sproutopia.Models;
using Sproutopia.Utilities;
using System.Text;

namespace SproutopiaTests;

[TestFixture]
public class GardenManagerTests
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
    [TestCase(7, 7, 3, 9, TestName = "New 3x3 garden")]
    [TestCase(7, 7, 2, 9, TestName = "New 3x3 garden from even numbered size")]
    [TestCase(0, 0, 3, 4, TestName = "New 3x3 garden clipped by two boundaries")]
    [TestCase(7, 0, 3, 6, TestName = "New 3x3 garden clipped by one boundary")]
    [TestCase(7, 7, 5, 25, TestName = "New 5x5 garden")]
    public void Test_InitialiseGarden(int x, int y, int size, int expectedArea)
    {
        // Arrange
        var gardenManager = new GardenManager(15, 15, _botManager, _randomizer);

        // Act
        var garden = gardenManager.InitialiseGarden(new Guid(), x, y, size);

        // Assert
        Assert.That(garden.ClaimedArea, Is.EqualTo(expectedArea));
    }

    [Test]
    [TestCase(null, null, null, null, null, null, "000000FF030303000000FF030303000000FF030303FFFFFFFFFFFFFF010101FF020202010101FF020202010101FF020202", TestName = "ViewGardens()")]
    [TestCase(3, 3, null, 7, 7, null, "000000FF030303000000FF030303000000FF030303FFFFFFFFFFFFFF010101FF020202010101FF020202010101FF020202", TestName = "ViewGardens(3, 3, 7, 7)")]
    [TestCase(3, 3, null, 6, 6, null, "000000FF030303000000FF030303000000FF030303FFFFFFFFFFFFFF010101FF020202010101FF020202010101FF020202", TestName = "ViewGardens(3, 3, 6, 6)")]
    [TestCase(3, 3, null, 5, 5, null, "0000FF03030000FF0303FFFFFFFFFF0101FF02020101FF0202", TestName = "ViewGardens(3, 3, 5, 5)")]
    [TestCase(2, 2, 2, null, null, null, "0000FF0000FFFFFFFF", TestName = "ViewGardens(2, 2, 2)")]
    [TestCase(2, 2, 3, null, null, null, "0000FF0000FFFFFFFF", TestName = "ViewGardens(2, 2, 3)")]
    [TestCase(2, 2, 4, null, null, null, "000000FF03000000FF03000000FF03FFFFFFFFFF010101FF02", TestName = "ViewGardens(2, 2, 4)")]
    [TestCase(2, 2, 5, null, null, null, "000000FF03000000FF03000000FF03FFFFFFFFFF010101FF02", TestName = "ViewGardens(2, 2, 5)")]
    [TestCase(0, 0, null, 5, 3, null, "FEFEFEFEFEFEFE0000FE0000FE0000", TestName = "ViewGardens(0, 0, 5, 3)")]
    [TestCase(10, 10, null, 3, 3, null, "FEFEFEFEFEFEFEFEFE", TestName = "ViewGardens(10, 10, 3, 3)")]
    [TestCase(null, null, null, null, null, 2, "000000FF03030300000007070303000400FF030303FF04FFFFFF06FF010101FF02060201010505020202010101FF020202", TestName = "ViewGardens({with trails in neutral territory})")]
    [TestCase(null, null, null, null, null, 4, "000000FF03030300070707070603000400FF030603FF04FFFFFF06FF010401FF02060201040505050502010101FF020202", TestName = "ViewGardens({with trails in opposition territory})")]
    public void Test_ViewGardens(int? x, int? y, int? size, int? width, int? height, int? trailLen, string expected)
    {
        // Arrange
        var gardenManager = new GardenManager(7, 7, new Guid[] {
            new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"),
            new("7542d58c-a331-4ac3-8947-e5b93780e7f1"),
            new("518887fc-d588-464c-952d-91307db2e412"),
            new("5a2d6764-8c3f-4fc6-9bee-d9d991b00d93"),
        }, _botManager, _randomizer);

            for (; (trailLen ?? 0) > 0; trailLen--)
            {
                _botManager.SetBotPosition(
                    new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"),
                    gardenManager.PerformAction(
                        new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"),
                        BotAction.Right)
                    .NewPosition!,
                    BotAction.IDLE);

                _botManager.SetBotPosition(
                    new("7542d58c-a331-4ac3-8947-e5b93780e7f1"),
                    gardenManager.PerformAction(
                        new("7542d58c-a331-4ac3-8947-e5b93780e7f1"),
                        BotAction.Down)
                    .NewPosition!,
                    BotAction.IDLE);

                _botManager.SetBotPosition(
                    new("518887fc-d588-464c-952d-91307db2e412"),
                    gardenManager.PerformAction(
                        new("518887fc-d588-464c-952d-91307db2e412"),
                        BotAction.Left)
                    .NewPosition!,
                    BotAction.IDLE);

                _botManager.SetBotPosition(
                    new("5a2d6764-8c3f-4fc6-9bee-d9d991b00d93"),
                    gardenManager.PerformAction(
                        new("5a2d6764-8c3f-4fc6-9bee-d9d991b00d93"),
                        BotAction.Up)
                    .NewPosition!,
                    BotAction.IDLE);
            }

        // Act
        CellType[][] map;
        if (x == null || y == null)
            map = gardenManager.ViewGardens();
        else if (size != null)
            map = gardenManager.ViewGardens((int)x, (int)y, (int)size);
        else
            map = gardenManager.ViewGardens((int)x, (int)y, (int)width!, (int)height!);

        StringBuilder sb = new StringBuilder();
        // Note: The following line of code serialises the map with the x/y axes flipped. It's not a problem as this is just a unit
        // test and it's not testing the array->string conversion but it might be confusing for someone trying to interpret the
        // expected and received result
        map.ToList().ForEach(x => x.ToList().ForEach(cell => sb.Append(((byte)cell).ToString("X2"))));

        // Assert
        Assert.That(sb.ToString(), Is.EqualTo(expected));
    }

    [Test]
    [TestCase(null, null, null, null, null, "0000000000000000011111000000001110000000000010000000000001000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", TestName = "ViewWeeds()")]
    [TestCase(5, 3, 5, null, null, "0111101110001000010000000", TestName = "ViewWeeds(5, 3, 5)")]
    [TestCase(5, 3, null, 7, 3, "001110000010000001000", TestName = "ViewWeeds(5, 3, 7, 3)")]
    public void Test_ViewWeeds(int? x, int? y, int? size, int? width, int? height, string expected)
    {
        // Arrange
        var gardenManager = new GardenManager(13, 13, new Guid[] {
            new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"),
            new("7542d58c-a331-4ac3-8947-e5b93780e7f1"),
            new("518887fc-d588-464c-952d-91307db2e412"),
            new("5a2d6764-8c3f-4fc6-9bee-d9d991b00d93"),
        }, _botManager, _randomizer);
        gardenManager.AddWeed(1);
        gardenManager.AddWeed(1);

        // Act
        bool[][] map;
        if (x == null || y == null)
            map = gardenManager.ViewWeeds();
        else if (size != null)
            map = gardenManager.ViewWeeds((int)x, (int)y, (int)size);
        else
            map = gardenManager.ViewWeeds((int)x, (int)y, (int)width!, (int)height!);

        StringBuilder sb = new StringBuilder();
        for (int yy = 0; yy < map[0].Length; yy++)
            for (int xx = 0; xx < map.Length; xx++)
                sb.Append(map[xx][yy]?"1":"0");

        Console.WriteLine(sb.ToString());

        // Assert
        Assert.That(sb.ToString(), Is.EqualTo(expected));
    }

    [Test]
    [TestCase(null, null, null, null, null, "7,7 Freeze|6,5 Freeze|5,5 TerritoryImmunity|1,0 TerritoryImmunity|4,1 Unprunable", TestName = "ViewPowerUps()")]
    [TestCase(4, 4, 7, null, null, "7,7 Freeze6,5 Freeze5,5 TerritoryImmunity4,1 Unprunable", TestName = "ViewPowerUps(4, 4, 7)")]
    [TestCase(4, 4, null, 5, 3, "6,5,Freeze|5,5,TerritoryImmunity", TestName = "ViewPowerUps(4, 4, 5, 3)")]
    public void Test_ViewPowerUps(int? x, int? y, int? size, int? width, int? height, string expected)
    {
        // Arrange
        var gardenManager = new GardenManager(9, 9, new Guid[] {
            new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"),
            new("7542d58c-a331-4ac3-8947-e5b93780e7f1"),
            new("518887fc-d588-464c-952d-91307db2e412"),
            new("5a2d6764-8c3f-4fc6-9bee-d9d991b00d93"),
        }, _botManager, _randomizer);
        gardenManager.AddPowerUp();
        gardenManager.AddPowerUp();
        gardenManager.AddPowerUp();
        gardenManager.AddPowerUp();
        gardenManager.AddPowerUp();

        // Act
        StringBuilder sb = new StringBuilder();
        if (x == null || y == null)
        {
            var response = gardenManager.ViewPowerUps<PowerUpType>();
            foreach (var p in response)
            {
                //Console.WriteLine($"{p.Coords} {p.PowerupType}");
                sb.Append($"{p.Coords} {p.PowerupType}|");
            }
        }
        else if (size != null)
        {
            var response = gardenManager.ViewPowerUps<PowerUpType>((int)x, (int)y, (int)size);
            foreach (var p in response)
            {
                sb.Append($"{p.Coords} {p.PowerupType}");
            }
        }
        else
        {
            var response = gardenManager.ViewPowerUps<PowerUpType>((int)x, (int)y, (int)width!, (int)height!);
            foreach (var p in response)
            {
                sb.Append($"{p.Coords},{p.PowerupType}|");
            }
        }

        // Assert
        Assert.That(sb.ToString().TrimEnd('|'), Is.EqualTo(expected));
    }

    [Test]
    [TestCase(0, 0, "23d7b429-4abf-4bb4-8a78-0ee0b55f74c0", TestName = "Top left garden")]
    [TestCase(3, 3, null, TestName = "No garden")]
    public void Test_GetGardenAtCell(int x, int y, Guid? expectedId)
    {
        // Arrange
        var gardenManager = new GardenManager(7, 7, new Guid[] {
            new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"),
            new("7542d58c-a331-4ac3-8947-e5b93780e7f1"),
            new("518887fc-d588-464c-952d-91307db2e412"),
            new("5a2d6764-8c3f-4fc6-9bee-d9d991b00d93"),
        }, _botManager, _randomizer);

        // Act
        var garden = gardenManager.GetGardenAtCell(new CellCoordinate(x, y));

        // Assert
        Assert.That(garden?.Id, Is.EqualTo(expectedId));
    }

    [Test]
    [TestCase(15, 15, 1, "23d7b429-4abf-4bb4-8a78-0ee0b55f74c0", TestName = "Fully enclosed")]
    [TestCase(15, 15, 3, "23d7b429-4abf-4bb4-8a78-0ee0b55f74c0", TestName = "Exact same space")]
    [TestCase(15, 15, 4, null, TestName = "Encloses other")]
    [TestCase(17, 15, 3, null, TestName = "Overlaps but not enclosed")]
    [TestCase(0, 0, 3, null, TestName = "Completely disjointed")]
    [TestCase(17, 0, 3, null, TestName = "Sides touching")]
    public void Test_GetEnclosingGarden(int x, int y, int size, string? expectedId)
    {
        GardenManager gardenManager = new GardenManager(30, 30, _botManager, _randomizer);
        var existingGarden = gardenManager.InitialiseGarden(new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"), 15, 15, 3);

        var newGarden = gardenManager.InitialiseGarden(new("7542d58c-a331-4ac3-8947-e5b93780e7f1"), x, y, size);
        if (expectedId == null)
        {
            Assert.That(gardenManager.GetEnclosingGarden(newGarden.Id), Is.Null);
        }
        else
        {
            var enclosingGarden = gardenManager.GetEnclosingGarden(newGarden.Id);
            Assert.That(enclosingGarden, Is.Not.Null);
            Assert.That(enclosingGarden.Id.ToString(), Is.EqualTo(expectedId));
        }
    }

    [Test]
    public void Test_GardenManagerHasGardenCompletedTrail()
    {
        GardenManager gardenManager = new GardenManager(30, 30, _botManager, _randomizer);
        var garden = gardenManager.InitialiseGarden(new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"), 0, 0, 3);

        Assert.That(gardenManager.HasGardenCompletedTrail(garden.Id), Is.False);

        garden.AddToTrail(new CellCoordinate(2, 0));
        Assert.That(gardenManager.HasGardenCompletedTrail(garden.Id), Is.False);

        garden.AddToTrail(new CellCoordinate(3, 0));
        Assert.That(gardenManager.HasGardenCompletedTrail(garden.Id), Is.False);

        garden.AddToTrail(new CellCoordinate(3, 1));
        Assert.That(gardenManager.HasGardenCompletedTrail(garden.Id), Is.False);

        garden.AddToTrail(new CellCoordinate(2, 1));
        Assert.That(gardenManager.HasGardenCompletedTrail(garden.Id), Is.False);

        garden.AddToTrail(new CellCoordinate(1, 1));
        Assert.That(gardenManager.HasGardenCompletedTrail(garden.Id), Is.True);
    }

    [Test]
    [TestCase("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0", 0, false, TestName = "Prune known garden")]
    [TestCase("7542d58c-a331-4ac3-8947-e5b93780e7f1", null, true, TestName = "Prune unknown garden")]
    public void Test_PruneGarden(string gardenId, int? expectedArea, bool expectException)
    {
        // Arrange
        var gardenManager = new GardenManager(15, 15, _botManager, _randomizer);
        gardenManager.InitialiseGarden(new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"), 7, 7, 5);

        // Act
        Garden? prunedGarden = null;
        bool gotException = false;
        try
        {
            prunedGarden = gardenManager.PruneGarden(new(gardenId));
        }
        catch { gotException = true; }

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(prunedGarden?.ClaimedArea, Is.EqualTo(expectedArea));
            Assert.That(gotException, Is.EqualTo(expectException));
        });
    }

    [Test]
    [TestCase("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0", "23d7b429-4abf-4bb4-8a78-0ee0b55f74c0", false, TestName = "Get known garden")]
    [TestCase("7542d58c-a331-4ac3-8947-e5b93780e7f1", null, true, TestName = "Get unknown garden")]
    public void Test_GetGarden(string gardenId, string? expectedId, bool expectException)
    {
        // Arrange
        var gardenManager = new GardenManager(15, 15, _botManager, _randomizer);
        gardenManager.InitialiseGarden(new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"), 7, 7, 5);

        // Act
        Garden? garden = null;
        bool gotException = false;
        try
        {
            garden = gardenManager.GetGardenById(new(gardenId));
        }
        catch { gotException = true; }

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(garden?.Id.ToString(), Is.EqualTo(expectedId));
            Assert.That(gotException, Is.EqualTo(expectException));
        });
    }

    [Test]
    [TestCase(2, 4, "23d7b429-4abf-4bb4-8a78-0ee0b55f74c0")]
    [TestCase(4, 5, "7542d58c-a331-4ac3-8947-e5b93780e7f1")]
    [TestCase(8, 6, null)]
    [TestCase(5, 0, "5a2d6764-8c3f-4fc6-9bee-d9d991b00d93")]
    public void Test_GetGardenTrail(int x, int y, string? expectedId)
    {
        // Arrange
        var gardenManager = new GardenManager(9, 9, _botManager, _randomizer);
        var garden0 = gardenManager.InitialiseGarden(new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"), 1, 1, 3);
        var garden1 = gardenManager.InitialiseGarden(new("7542d58c-a331-4ac3-8947-e5b93780e7f1"), 1, 7, 3);
        var garden2 = gardenManager.InitialiseGarden(new("518887fc-d588-464c-952d-91307db2e412"), 7, 7, 3);
        var garden3 = gardenManager.InitialiseGarden(new("5a2d6764-8c3f-4fc6-9bee-d9d991b00d93"), 7, 1, 3);
        garden0.AddToTrail(new CellCoordinate(0, 3));
        garden0.AddToTrail(new CellCoordinate(0, 4));
        garden0.AddToTrail(new CellCoordinate(3, 4));
        garden1.AddToTrail(new CellCoordinate(3, 8));
        garden1.AddToTrail(new CellCoordinate(4, 8));
        garden1.AddToTrail(new CellCoordinate(4, 5));
        garden3.AddToTrail(new CellCoordinate(5, 0));
        garden3.AddToTrail(new CellCoordinate(4, 0));
        garden3.AddToTrail(new CellCoordinate(4, 3));

        // Act
        var garden = gardenManager.GetGardenTrail(new(x, y));

        // Assert
        Assert.That(garden?.Id.ToString(), Is.EqualTo(expectedId));
    }

    private record Territory(int x, int y);
    static Territory[] t1 = new Territory[] { new(1, 1) }; // top left corner
    static Territory[] t2 = new Territory[] { new(1, 7) }; // bottom left corner
    static Territory[] t3 = new Territory[] { new(7, 7), new(7, 1) }; // bottom right and top right
    static Territory[] t4 = new Territory[] { new(3, 3) }; // overlapping with t1

    [Test]
    [TestCase("1,1", "1,7", "1,1|1,7", TestName = "Two disjoint territories")]
    [TestCase("1,1", "1,4", "1,1|1,4", TestName = "Two adjacent territories")]
    [TestCase("1,1", "3,3", "1,1|3,3", TestName = "Two overlapping territories")]
    [TestCase("1,1|1,7", "7,7", "1,1|1,7|7,7", TestName = "Transfer dual territory to single territory")]
    [TestCase("7,7", "1,1|1,7", "7,7|1,1|1,7", TestName = "Transfer single territory to dual territory")]
    public void Test_TransferGarden(string gardenSrcOrigin, string gardenDstOrigin, string expectedArea)
    {
        // Arrange
        var gardenManager = new GardenManager(9, 9, _botManager, _randomizer);
        var gardenSrc = gardenManager.InitialiseGarden(new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"), 1, 1, 3);
        var gardenDst = gardenManager.InitialiseGarden(new("7542d58c-a331-4ac3-8947-e5b93780e7f1"), 1, 7, 3);
        gardenSrc.HomeBase = new Polygon(null);
        gardenDst.HomeBase = new Polygon(null);

        Geometry geometrySrc = new Polygon(null);
        Geometry geometryDst = new Polygon(null);
        Geometry geometryExpected = new Polygon(null);

        gardenSrcOrigin.Split('|').ToList().ForEach(coords => {
            var x = int.Parse(coords.Split(",")[0]);
            var y = int.Parse(coords.Split(",")[1]);
            geometrySrc = geometrySrc.Union(new Polygon(new LinearRing(new Coordinate[] {
                new(x-1,y-1),
                new(x+1,y-1),
                new(x+1,y+1),
                new(x-1,y+1),
                new(x-1,y-1),
            })));
        });

        gardenDstOrigin.Split('|').ToList().ForEach(coords => {
            var x = int.Parse(coords.Split(",")[0]);
            var y = int.Parse(coords.Split(",")[1]);
            geometryDst = geometryDst.Union(new Polygon(new LinearRing(new Coordinate[] {
                new(x-1,y-1),
                new(x+1,y-1),
                new(x+1,y+1),
                new(x-1,y+1),
                new(x-1,y-1),
            })));
        });

        expectedArea.Split('|').ToList().ForEach(coords => {
            var x = int.Parse(coords.Split(",")[0]);
            var y = int.Parse(coords.Split(",")[1]);
            geometryExpected = geometryExpected.Union(new Polygon(new LinearRing(new Coordinate[] {
                new(x-1,y-1),
                new(x+1,y-1),
                new(x+1,y+1),
                new(x-1,y+1),
                new(x-1,y-1),
            })));
        });

        (gardenSrc as Garden)!.ClaimedLand = geometrySrc;
        (gardenDst as Garden)!.ClaimedLand = geometryDst;

        // Act
        gardenManager.TransferGarden(gardenSrc.Id, gardenDst.Id);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That((gardenDst as Garden)!.ClaimedLand.Covers(geometryExpected), Is.EqualTo(true));
            Assert.That(geometryExpected.Covers((gardenDst as Garden)!.ClaimedLand), Is.EqualTo(true));
        });
    }

    [Test]
    [TestCase("1,1|1,7", "1,2|1,3|1,4|1,5|1,6", 21, TestName = "Connect disjoint territories")]
    [TestCase("1,1|1,7", "0,2|0,3|0,4|0,5|0,6/2,2|2,3|2,4|2,5|2,6", 27, TestName = "Connect disjoint territories twice")]
    [TestCase("1,1|1,7", "2,1|3,1|4,1|4,2|4,3|4,4|3,4|2,4|1,4|1,3|1,2", 30, TestName = "Connect to same territory")]
    public void Test_CompleteTrail(string territories, string trail, int expectedArea)
    {
        // Arrange
        var gardenManager = new GardenManager(9, 9, _botManager, _randomizer);
        var garden = gardenManager.InitialiseGarden(new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"), 0, 0, 0);
        Geometry geometryTerritories = new Polygon(null);

        territories.Split('|').ToList().ForEach(coords => {
            var x = int.Parse(coords.Split(",")[0]);
            var y = int.Parse(coords.Split(",")[1]);
            geometryTerritories = geometryTerritories.Union(new Polygon(new LinearRing(new Coordinate[] {
                new(x-1,y-1),
                new(x+2,y-1),
                new(x+2,y+2),
                new(x-1,y+2),
                new(x-1,y-1),
            })));
        });
        (garden as Garden)!.ClaimedLand = geometryTerritories;

        foreach (var t in trail.Split('/'))
        {
            t.Split('|').ToList().ForEach(coords =>
            {
                var cell = new CellCoordinate(
                    int.Parse(coords.Split(",")[0]),
                    int.Parse(coords.Split(",")[1]));
                garden.AddToTrail(cell);
            });

            // Act
            gardenManager.CompleteTrail(new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"));
        }

        Console.WriteLine(garden);

        // Assert
        Assert.That(garden.ClaimedArea, Is.EqualTo(expectedArea));
    }

    [Test]
    [TestCase("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0", 2, 1, 9, TestName = "Top Left")]
    [TestCase("7542d58c-a331-4ac3-8947-e5b93780e7f1", 7, 2, 9, TestName = "Top Right")]
    [TestCase("518887fc-d588-464c-952d-91307db2e412", 6, 7, 9, TestName = "Bottom Right")]
    [TestCase("5a2d6764-8c3f-4fc6-9bee-d9d991b00d93", 1, 6, 9, TestName = "Bottom Left")]
    public void Test_RespawnBot(string botId, int expectedX, int expectedY, int expectedArea)
    {
        // Arrange
        var gardenManager = new GardenManager(9, 9, [
            new("23d7b429-4abf-4bb4-8a78-0ee0b55f74c0"),
            new("7542d58c-a331-4ac3-8947-e5b93780e7f1"),
            new("518887fc-d588-464c-952d-91307db2e412"),
            new("5a2d6764-8c3f-4fc6-9bee-d9d991b00d93")
        ], _botManager, _randomizer);

        // Act
        var botResponse = gardenManager.RespawnBot(new(botId));
        var newGarden = gardenManager.GetGardenById(new(botId));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(botResponse.NewPosition, Is.EqualTo(new CellCoordinate(expectedX, expectedY)));
            Assert.That(newGarden.ClaimedArea, Is.EqualTo(expectedArea));
        });
    }

    [Test]
    [TestCaseSource(nameof(ReadTestCaseFiles))]
    public void TestPerformActionWithFiles(TestData testData)
    {
        Dictionary<char, Guid> id = new()
        {
            ['A'] = Guid.NewGuid(),
            ['B'] = Guid.NewGuid(),
            ['C'] = Guid.NewGuid(),
            ['D'] = Guid.NewGuid()
        };

        // Arrange
        var gardenManager = new GardenManager(testData.start.Length, testData.start[0].Length, _botManager, _randomizer);
        gardenManager.InitialiseGarden(id['A'], testData.startingPositions[0].X, testData.startingPositions[0].Y, 0);
        gardenManager.InitialiseGarden(id['B'], testData.startingPositions[1].X, testData.startingPositions[1].Y, 0);
        gardenManager.InitialiseGarden(id['C'], testData.startingPositions[2].X, testData.startingPositions[2].Y, 0);
        gardenManager.InitialiseGarden(id['D'], testData.startingPositions[3].X, testData.startingPositions[3].Y, 0);

        _botManager.AddBot(new BotState(id['A'], testData.startingPositions[0]));
        _botManager.AddBot(new BotState(id['B'], testData.startingPositions[1]));
        _botManager.AddBot(new BotState(id['C'], testData.startingPositions[2]));
        _botManager.AddBot(new BotState(id['D'], testData.startingPositions[3]));

        var garden0 = gardenManager.GetGardenById(id['A']);
        var garden1 = gardenManager.GetGardenById(id['B']);
        var garden2 = gardenManager.GetGardenById(id['C']);
        var garden3 = gardenManager.GetGardenById(id['D']);

        for (int x = 0; x < testData.start.Length; x++)
            for (int y = 0; y < testData.start[0].Length; y++)
            {
                switch (testData.start[x][y])
                {
                    case CellType.Bot0Territory:
                        garden0.ClaimedLand = garden0.ClaimedLand.Union(new CellCoordinate(x, y).ToCellInPointCoordinateSystem());
                        break;
                    case CellType.Bot1Territory:
                        garden1.ClaimedLand = garden1.ClaimedLand.Union(new CellCoordinate(x, y).ToCellInPointCoordinateSystem());
                        break;
                    case CellType.Bot2Territory:
                        garden2.ClaimedLand = garden2.ClaimedLand.Union(new CellCoordinate(x, y).ToCellInPointCoordinateSystem());
                        break;
                    case CellType.Bot3Territory:
                        garden3.ClaimedLand = garden3.ClaimedLand.Union(new CellCoordinate(x, y).ToCellInPointCoordinateSystem());
                        break;
                }
            }

        // Act
        BotResponse botResponse = new BotResponse();
        foreach (var (bot, action) in testData.actions)
        {
            _botManager.GetBotState(id[bot]).EnqueueCommand(new SproutBotCommand(id[bot], action));
            _botManager.GetBotState(id[bot]).DequeueCommand();

            botResponse = gardenManager.PerformAction(id[bot], action);
            if (botResponse.NewPosition != null)
            {
                _botManager.SetBotPosition(id[bot], botResponse.NewPosition, botResponse.Momentum);
            }
        }
        var finalWorld = gardenManager.ViewGardens();

        Console.WriteLine(gardenManager.ToString());

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(Helpers.JaggedArraysEqual(finalWorld!, testData.expected), Is.True);
            Assert.That(botResponse.BotsPruned.Order(), Is.EqualTo(testData.prunedBots.Select(b => id[b]).Order()));
        });
    }

    public class TestData
    {
        public required string fileName;
        public required CellType[][] start;
        public required CellCoordinate[] startingPositions;
        public required List<(char bot, BotAction action)> actions;
        public required CellType[][] expected;
        public required List<char> prunedBots;

        public override string ToString()
        {
            return fileName;
        }
    }

    private static TestData[] ReadTestCaseFiles()
    {
        List<TestData> testData = [];
        Dictionary<char, CellType> lookupCellType = new()
        {
            ['A'] = CellType.Bot0Territory,
            ['B'] = CellType.Bot1Territory,
            ['C'] = CellType.Bot2Territory,
            ['D'] = CellType.Bot3Territory,
            ['a'] = CellType.Bot0Trail,
            ['b'] = CellType.Bot1Trail,
            ['c'] = CellType.Bot2Trail,
            ['d'] = CellType.Bot3Trail,
            ['.'] = CellType.Unclaimed
        };
        Dictionary<char, BotAction> lookupAction = new()
        {
            ['S'] = BotAction.Down,
            ['N'] = BotAction.Up,
            ['E'] = BotAction.Right,
            ['W'] = BotAction.Left
        };

        foreach (string file in Directory.EnumerateFiles("PerformActionTestFiles", "*.txt"))
        {
            var lines = File.ReadAllLines(file).ToList();
            List<int> emptyIndices = lines
                .Select((value, index) => new { value, index })
                .Where(item => string.IsNullOrEmpty(item.value))
                .Select(item => item.index)
                .ToList();

            // Read starting world
            var start = Helpers.CreateJaggedArray<CellType[][]>(lines[1].Length, emptyIndices[0] - 1);
            for (int i = 1; i < emptyIndices[0]; i++)
            {
                for (int j = 0; j < lines[i].Length; j++)
                {
                    start[j][i - 1] = lookupCellType[lines[i][j]];
                }
            }

            // Read starting bot positions
            List<CellCoordinate> startingPositions = [];
            var positionsLine = lines.First(s => s.StartsWith("StartingPositions:")).Split(':', StringSplitOptions.TrimEntries)[1];
            foreach (string s in positionsLine.Split('|'))
            {
                var x = int.Parse(s.Split(',')[0]);
                var y = int.Parse(s.Split(',')[1]);
                startingPositions.Add(new(x, y));
            }

            // Read actions
            List<(char bot, BotAction action)> actions = [];
            var actionsLine = lines.First(s => s.StartsWith("Actions:")).Split(':', StringSplitOptions.TrimEntries)[1];
            foreach (var s in actionsLine.Split(','))
            {
                actions.Add((s[0], lookupAction[s[1]]));
            }

            // Read expected world
            var expectedLineIndex = lines.Select((value, index) => new { value, index }).FirstOrDefault(item => item.value.StartsWith("Expected:"))!.index;
            var expected = Helpers.CreateJaggedArray<CellType[][]>(lines[expectedLineIndex + 1].Length, emptyIndices[3] - expectedLineIndex - 1);
            for (int i = expectedLineIndex + 1; i < emptyIndices[3]; i++)
            {
                for (int j = 0; j < lines[i].Length; j++)
                {
                    expected[j][i - expectedLineIndex - 1] = lookupCellType[lines[i][j]];
                }
            }

            // Read pruned bots list
            List<char> prunedBots = [];
            var prunedBotsLine = (lines.FirstOrDefault(s => s.StartsWith("PrunedBots:")) ?? ":").Split(':', StringSplitOptions.TrimEntries)[1];
            foreach (var s in prunedBotsLine.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                prunedBots.Add(s[0]);
            }

            testData.Add(new TestData
            {
                fileName = file,
                start = start,
                startingPositions = startingPositions.ToArray(),
                actions = actions,
                expected = expected,
                prunedBots = prunedBots,
            });
        }

        return testData.ToArray();
    }
}