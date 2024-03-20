using Microsoft.Extensions.Options;
using Sproutopia.Models;

public class GlobalSeededRandomizer
{
    public readonly Random random;

    public GlobalSeededRandomizer(IOptions<SproutopiaGameSettings> gameSettings)
    {
        random = new Random(gameSettings.Value.Seed);
    }

    public int Next()
    {
        return random.Next();
    }

    public int Next(int maxValue)
    {
        return random.Next(maxValue);
    }

    /// <summary>
    /// Returns normally distributed random number
    /// </summary>
    /// <param name="mean">Mean of normal distribution</param>
    /// <param name="stdDev">Standard deviation of normal distribution</param>
    /// <param name="min">Minimum returned value</param>
    /// <param name="max">Maximum returned value</param>
    /// <returns>double</returns>
    public double NextNormal(double mean, double stdDev, double min, double max)
    {
        double u1 = 1.0 - random.NextDouble();
        double u2 = 1.0 - random.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        double randNormal = mean + stdDev * randStdNormal;

        // Clip the result between min and max
        return Math.Max(Math.Min(randNormal, max), min);
    }
}
