using Microsoft.AspNetCore.Http;

namespace Student_Management_API.DTOs
{
    public class FileUploadDto
    {
        public IFormFile File { get; set; }
    }
}
