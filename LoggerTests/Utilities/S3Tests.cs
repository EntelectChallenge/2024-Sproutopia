using Logger.Utilities;

namespace LoggerTests.Utilities
{
    [TestFixture]
    public class S3Tests
    {
        [Test]
        public void GivenNoEnvironmentVariable_ShouldThrowException()
        {
            // Assert
            Assert.That(() => S3.UploadLogs().Wait(), Throws.TypeOf<AggregateException>().With.Message.EqualTo("One or more errors occurred. (No LOG_DIR environment variable)"));
        }
    }
}
