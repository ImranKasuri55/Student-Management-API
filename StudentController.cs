using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using Student_Management_API.Data;
using Student_Management_API.DTOs;
using Student_Management_API.Modles;

namespace Student_Management_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔐 PROTECT ENTIRE CONTROLLER
    public class StudentController : ControllerBase
    {
        private readonly SchoolContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public StudentController(
            SchoolContext context,
            IMapper mapper,
            IMemoryCache cache)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
        }

        // ================= POLICY BASED =================
        [Authorize(Policy = "AtLeast18")]
        [HttpGet("secure")]
        public IActionResult SecureEndpoint()
        {
            return Ok("Age verified");
        }

        // ================= READ (ADMIN + USER) =================
        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public IActionResult GetStudents()
        {
            if (!_cache.TryGetValue("students", out List<StudentReadDto> students))
            {
                var data = _context.Students.ToList();
                students = _mapper.Map<List<StudentReadDto>>(data);

                _cache.Set("students", students, TimeSpan.FromMinutes(5));
            }

            return Ok(students);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}")]
        public IActionResult GetStudent(int id)
        {
            Log.Information("Fetching student with ID {Id}", id);

            var student = _context.Students.Find(id);
            if (student == null)
                return NotFound(new { message = "Student not found" });

            return Ok(_mapper.Map<StudentReadDto>(student));
        }

        // ================= CREATE (ADMIN ONLY) =================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AddStudent(StudentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var student = _mapper.Map<Student>(dto);
            student.CreatedAt = DateTime.Now;

            _context.Students.Add(student);
            _context.SaveChanges();
            _cache.Remove("students");

            Log.Information("Student created: {Name}", dto.Name);
            return Ok(new { message = "Student added successfully" });
        }

        // ================= UPDATE (ADMIN ONLY) =================
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult UpdateStudent(int id, StudentUpdateDto dto)
        {
            var student = _context.Students.Find(id);
            if (student == null)
                return NotFound(new { message = "Student not found" });

            _mapper.Map(dto, student);
            _context.SaveChanges();
            _cache.Remove("students");

            Log.Information("Student updated with ID {Id}", id);
            return Ok(new { message = "Student updated successfully" });
        }

        // ================= DELETE (ADMIN ONLY) =================
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            var student = _context.Students.Find(id);
            if (student == null)
                return NotFound(new { message = "Student not found" });

            _context.Students.Remove(student);
            _context.SaveChanges();
            _cache.Remove("students");

            Log.Information("Student deleted with ID {Id}", id);
            return Ok(new { message = "Student deleted successfully" });
        }
    }
}
