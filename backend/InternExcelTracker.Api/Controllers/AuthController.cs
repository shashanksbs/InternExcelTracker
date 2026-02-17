using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InternExcelTracker.Api.Data;
using InternExcelTracker.Api.Models;
using InternExcelTracker.Api.DTOs;

namespace InternExcelTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------------
        // REGISTER
        // ---------------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                return BadRequest(new { Message = "Passwords do not match." });

            if (await _context.Users.AnyAsync(u => 
                u.Username == dto.Username || u.Email == dto.Email))
            {
                return BadRequest(new { Message = "Username or Email already exists." });
            }

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Role = dto.Role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User registered successfully." });
        }

        // ---------------------------
        // LOGIN
        // ---------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || 
                !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { Message = "Invalid credentials." });
            }

            if (user.Role != dto.Role)
            {
                return Unauthorized(new { Message = "Role mismatch." });
            }

            return Ok(new LoginResponseDto
            {
                Username = user.Username,
                Role = user.Role,
                Message = "Login successful."
            });
        }
    }
}
