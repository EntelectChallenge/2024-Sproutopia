using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace Logger.Utilities
{
    public static class S3
    {
        public static async Task UploadLogs()
        {
            Console.WriteLine("Game Complete. Saving logs...");
            var LOG_DIR = Environment.GetEnvironmentVariable("LOG_DIR") ?? throw new Exception("No LOG_DIR environment variable");

            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PUSH_LOGS_TO_S3")))
            {
                return;
            }

            try
            {
                var fullS3Path = Environment.GetEnvironmentVariable("S3_BUCKET_NAME");
                var logDirectory = Path.GetFullPath(Path.Combine("..", LOG_DIR));

                if (string.IsNullOrWhiteSpace(fullS3Path)) return;

                string[] parts = fullS3Path.Split('/');
                var bucketName = parts[0];
                string prefix = "/" + string.Join("/", parts.Skip(1));
                var bucketRegion = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION"));

                Console.WriteLine("Beginning S3 Upload");
                var s3Client = new AmazonS3Client(bucketRegion);
                var transferUtility = new TransferUtility(s3Client);
                TransferUtilityUploadDirectoryRequest uploadRequest = new()
                {
                    BucketName = bucketName,
                    Directory = logDirectory,
                    KeyPrefix = prefix
                };
                await transferUtility.UploadDirectoryAsync(uploadRequest);
                Console.WriteLine("Completed S3 Upload");
            }
            catch (Exception exp)
            {
                Console.WriteLine($"Failed to upload to S3 - {exp.Message}");
            }
        }
    }
}
