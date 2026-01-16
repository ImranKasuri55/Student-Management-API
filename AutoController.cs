using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Management_API.Data;
using Student_Management_API.DTOs;
using Student_Management_API.Helpers;
using Student_Management_API.Models;
using Student_Management_API.Services;

namespace Student_Management_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly SchoolContext _context;
        private readonly ITokenService _tokenService;

        public AuthController(SchoolContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // ✅ REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(x => x.Username == dto.Username))
                return BadRequest("Username already exists");

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = PasswordHelper.HashPassword(dto.Password),
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully");
        }

        // ✅ LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var hash = PasswordHelper.HashPassword(dto.Password);

            var user = await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.Username == dto.Username &&
                    x.PasswordHash == hash);

            if (user == null)
                return Unauthorized("Invalid username or password");

            var token = _tokenService.CreateToken(user);

            return Ok(new { token });
        }
    }
}
