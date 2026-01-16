using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Student_Management_API.Modles
{
    public class Student
    {
        public  int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(100)]
        public string Email { get; set; }
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        [Range(0,100)]
        public int Marks { get; set; }
        
        public DateTime CreatedAt { get; set; }

    }
}
