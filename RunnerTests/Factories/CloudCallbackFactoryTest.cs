using Domain.Enums;
using NUnit.Framework;
using Runner.Factories;

namespace RunnerTests.Factories
{
    [TestFixture]
    public class CloudCallbackFactoryTest
    {
        Dictionary<CloudCallbackType, string> lookup = new Dictionary<CloudCallbackType, string>() { 
            { 
                CloudCallbackType.Initializing, "initializing"
            },
            {
                CloudCallbackType.Ready, "ready"
            },
            {
                CloudCallbackType.Started, "started"
            },
            {
                CloudCallbackType.Failed, "failed"
            },
            {
                CloudCallbackType.Finished, "finished"
            },
            {
                CloudCallbackType.LoggingComplete, "logging_complete"
            }
        };

        [Test]
        public void GivenEachCallbackType_ShouldBuildTheRightCloudPayload()
        {
            CloudCallbackType[] callbackTypes = (CloudCallbackType[])Enum.GetValues(typeof(CloudCallbackType));

            foreach (var callbackType in callbackTypes)
            {
                // Act
                var callbackUnderTest = CloudCallbackFactory.Build("123", callbackType, null, 0, 0);

                // Assert
                Assert.That(callbackUnderTest.MatchStatus, Does.Contain(lookup[callbackType]));
            }
        }
    }
}
