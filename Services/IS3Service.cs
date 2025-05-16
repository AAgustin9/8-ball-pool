namespace _8_ball_pool.Services
{
    public interface IS3Service
    {
        Task<string> GeneratePresignedUrlAsync(int userId, string fileName, string contentType);
    }
}