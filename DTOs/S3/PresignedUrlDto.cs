namespace _8_ball_pool.DTOs.S3
{
    public class PresignedUrlDto
    {
        public required string UploadUrl { get; set; }
        public required string FileUrl { get; set; }
    }
} 