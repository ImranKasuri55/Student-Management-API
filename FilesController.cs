using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student_Management_API.DTOs;

namespace Student_Management_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        [Authorize(Roles = "Admin,User")]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("No file uploaded");

            // Allowed file types
            var allowedTypes = new[] { "image/jpeg", "image/png", "application/pdf" };
            if (!allowedTypes.Contains(dto.File.ContentType))
                return BadRequest("Only JPG, PNG, or PDF allowed");

            // Max size: 2MB
            if (dto.File.Length > 2 * 1024 * 1024)
                return BadRequest("File size must be <= 2MB");

            var uploadsPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads"
            );

            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var fileName = Guid.NewGuid() + Path.GetExtension(dto.File.FileName);
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
            return Ok(new { fileUrl });
        }
    }
}
