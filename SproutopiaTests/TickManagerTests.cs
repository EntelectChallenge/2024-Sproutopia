using Microsoft.Extensions.Options;
using Sproutopia.Managers;
using Sproutopia.Models;

namespace SproutopiaTests;

[TestFixture]
public class TickManagerTests
{
    private TickManager tickManager;
    private IOptions<SproutopiaGameSettings> settings = Options.Create(new SproutopiaGameSettings() { TickRate = 150 });

    [SetUp]
    public void Setup()
    {
        tickManager = new TickManager(settings);
    }

    [Test]
    public void Test_StartInitialTimer()
    {
        tickManager.StartTimer();
        Assert.That(tickManager.ShouldContinue(), Is.True);
        Assert.That(tickManager.CurrentTick, Is.EqualTo(1));

    }

    [Test]
    public void Test_OnPauseShouldStopTimer()
    {
        tickManager.StartTimer();
        tickManager.Pause();
        Assert.That(tickManager.ShouldContinue(), Is.False);
        Assert.That(tickManager.CurrentTick, Is.EqualTo(0));
    }

    [Test]
    public void Test_OnStepShouldStopTimer()
    {
        tickManager.StartTimer();
        tickManager.Step();
        Assert.That(tickManager.ShouldContinue(), Is.True);
        Assert.That(tickManager.CurrentTick, Is.EqualTo(1));
        Assert.That(tickManager.ShouldContinue(), Is.False);

    }

    [Test]
    public void Test_OnContinueShouldRestartTimer()
    {
        tickManager.StartTimer();
        tickManager.Pause();
        Assert.That(tickManager.ShouldContinue(), Is.False);
        tickManager.Continue();
        Assert.That(tickManager.ShouldContinue(), Is.True);

    }

    [Test]
    [TestCase("0")]
    [TestCase("5")]
    public void Test_ShouldContinueWaitUntilElapsedTime(int elapsedTime)
    {
        tickManager.StartTimer();
        Assert.That(tickManager.ShouldContinue(), Is.True);
        Thread.Sleep(elapsedTime);
        Assert.That(tickManager.ShouldContinue(), Is.False);
    }

    [Test]
    [TestCase("150")]
    [TestCase("160")]
    public void Test_ShouldContinueTickIncreasedAfterTheElapsedTime(int elapsedTime)
    {
        tickManager.StartTimer();
        Assert.That(tickManager.ShouldContinue(), Is.True);
        Thread.Sleep(elapsedTime);
        Assert.Multiple(() =>
        {
            Assert.That(tickManager.CurrentTick, Is.EqualTo(1));
            Assert.That(tickManager.ShouldContinue(), Is.True);
        });
    }
    
}

