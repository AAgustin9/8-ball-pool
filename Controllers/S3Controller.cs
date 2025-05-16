using Microsoft.AspNetCore.Mvc;
using _8_ball_pool.Services;

namespace _8_ball_pool.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class S3Controller : ControllerBase
    {
        private readonly IS3Service _s3Service;

        public S3Controller(IS3Service s3Service)
        {
            _s3Service = s3Service;
        }

        [HttpGet("presigned-url")]
        public async Task<IActionResult> GetPresignedUrl([FromQuery] int userId, [FromQuery] string fileName, [FromQuery] string contentType)
        {
            var url = await _s3Service.GeneratePresignedUrlAsync(userId, fileName, contentType);
            return Ok(new { uploadUrl = url });
        }
    }
}