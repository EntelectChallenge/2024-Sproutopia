using Domain;
using Sproutopia.Domain;
using Sproutopia.Enums;
using Sproutopia.Managers;
using Sproutopia.Models;

namespace SproutopiaTests.Mocks;

/// <summary>
/// Mock implementation of the IGardenManager interface for testing purposes.
/// Presently, this implementation has very little actual logic and most methods
/// will simply throw a NotImplemented exception. As unit tests are added that
/// require any other methods to be implemented, those can be fleshed out. But
/// remember, the point of this implementation is to test functionality in other
/// classes in the project (like GameState) which requires an injected
/// IGardenManager, not the functionality of the GardenManager (those tests are
/// in GardenManagerTests.cs. So there should be little need for actual logic in
/// this implementation.
/// </summary>
public class MockGardenManager : IGardenManager
{
    private List<Guid> _powerUps = [];
    private List<Guid> _superPowerUps = [];

    public BotResponse _mockedPerformActionResponse { get; set; }

    public MockGardenManager(BotResponse mockedPerformActionResponse)
    {
        _mockedPerformActionResponse = mockedPerformActionResponse;
    }

    public void AddBot(Guid botId, string nickname, string connectionId)
    {
        throw new NotImplementedException();
    }

    public void AddPowerUp()
    {
        throw new NotImplementedException();
    }

    public void AddPowerUp(Guid powerUpId)
    {
        _powerUps.Add(powerUpId);
    }

    public void AddWeed(int growthRate)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Weed> GetWeeds()
    {
        throw new NotImplementedException();
    }

    public int GrowWeed(Guid weedId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<(Guid botId, int claimedPercentage)> Leaderboard()
    {
        throw new NotImplementedException();
    }

    public BotResponse PerformAction(Guid botId, BotAction action)
    {
        return _mockedPerformActionResponse;
    }

    public int PowerUpCount()
    {
        return _powerUps.Count;
    }

    public void RemovePowerUp<T>(Guid powerUpId) where T : Enum
    {
        if (typeof(T) == typeof(PowerUpType))
        {
            _powerUps.Remove(powerUpId);
        }
        else if (typeof(T) == typeof(SuperPowerUpType))
        {
            _superPowerUps.Remove(powerUpId);
        }
        else
        {
            throw new ArgumentException("Unknown PowerUp type specified", nameof(T));
        }
    }

    public BotResponse RespawnBot(Guid botId)
    {
        throw new NotImplementedException();
    }

    public CellType[][] ViewGardens()
    {
        throw new NotImplementedException();
    }

    public CellType[][] ViewGardens(int x, int y, int size)
    {
        throw new NotImplementedException();
    }

    public CellType[][] ViewGardens(int x, int y, int width, int height)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<(CellCoordinate Coords, T PowerupType)> ViewPowerUps<T>() where T : Enum
    {
        throw new NotImplementedException();
    }

    public IEnumerable<(CellCoordinate Coords, T PowerupType)> ViewPowerUps<T>(int x, int y, int size) where T : Enum
    {
        throw new NotImplementedException();
    }

    public IEnumerable<(CellCoordinate Coords, T PowerupType)> ViewPowerUps<T>(int x, int y, int width, int height) where T : Enum
    {
        throw new NotImplementedException();
    }

    public bool[][] ViewWeeds()
    {
        throw new NotImplementedException();
    }

    public bool[][] ViewWeeds(int x, int y, int size)
    {
        throw new NotImplementedException();
    }

    public bool[][] ViewWeeds(int x, int y, int width, int height)
    {
        throw new NotImplementedException();
    }

    public int WeedCount()
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return "Mocked GardenManager";
    }

    public Dictionary<CellCoordinate, PowerUpType> GetPowerUpTypes()
    {
        throw new NotImplementedException();
    }

    public Dictionary<CellCoordinate, SuperPowerUpType> GetSuperPowerUpTypes()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<CellCoordinate> GetGardenCellsById(Guid gardenId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<CellCoordinate> GetTrailCellsById(Guid gardenId)
    {
        throw new NotImplementedException();
    }
}
