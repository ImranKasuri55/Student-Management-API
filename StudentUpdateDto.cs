using Microsoft.Identity.Client;

namespace Student_Management_API.DTOs
{
    public class StudentUpdateDto
    { 
        public string Name { get; set; }
        public int Marks { get; set; }
    }
}
