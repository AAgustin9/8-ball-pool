using Amazon.S3;
using Amazon.S3.Model;

namespace _8_ball_pool.Services
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _region;

        public S3Service(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = Environment.GetEnvironmentVariable("AWS_BUCKET_NAME") 
                ?? throw new InvalidOperationException("AWS_BUCKET_NAME not set");
            _region = Environment.GetEnvironmentVariable("AWS_REGION") 
                ?? throw new InvalidOperationException("AWS_REGION not set");
        }

        public async Task<string> GeneratePresignedUrlAsync(int userId, string fileName, string contentType)
        {
            var key = $"users/{userId}/profile-pictures/{Guid.NewGuid()}-{fileName}";
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMinutes(15),
                ContentType = contentType
            };

            return _s3Client.GetPreSignedURL(request);
        }
    }
}