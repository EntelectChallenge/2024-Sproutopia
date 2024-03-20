using Microsoft.Extensions.Options;
using Sproutopia.Models;

namespace SproutopiaTests;

[TestFixture]
public class UtilitiesTests
{
    private GlobalSeededRandomizer _randomizer = new GlobalSeededRandomizer(Options.Create(new SproutopiaGameSettings() { Seed = 123 }));

    [Test]
    [TestCase(0, 4.0, -20, 20, TestName = "Smooth around zero")]
    [TestCase(20, 4.0, 0, 40, TestName = "Smooth around 20")]
    [TestCase(0, 1.0, -5, 5, TestName = "Sharp around zero")]
    [TestCase(10, 1.0, 5, 15, TestName = "Sharp around 5")]
    public void Test_NextNormal(double mean, double stddev, double min, double max)
    {
        // Arrange
        var errMarginMean = 0.1;
        var errMarginStdDev = 0.1;

        // Act
        var randomNumbers = Enumerable.Range(0, 1000)
                                      .Select(_ => _randomizer.NextNormal(mean, stddev, min, max))
                                      .ToArray();

        var calcMean = randomNumbers.Average();
        var calcStdDev = Math.Sqrt(randomNumbers.Average(v => Math.Pow(v - mean, 2)));
        //foreach (var n in randomNumbers)
        //    Console.WriteLine(n);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(calcMean, Is.InRange(mean - errMarginMean, mean + errMarginMean));
            Assert.That(calcStdDev, Is.InRange(stddev * (1 - errMarginStdDev), stddev * (1 + errMarginStdDev)));
        });

    }
}