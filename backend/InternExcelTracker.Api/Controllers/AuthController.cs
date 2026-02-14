using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InternExcelTracker.Api.Data;
using InternExcelTracker.Api.Models;
using InternExcelTracker.Api.DTOs;
using BCrypt.Net;

namespace InternExcelTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly Services.ILoggerService _logger;

        public AuthController(ApplicationDbContext context, Services.ILoggerService logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                return BadRequest(new { Message = "Passwords do not match." });

            if (await _context.Users.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email))
                return BadRequest(new { Message = "Username or Email already exists." });

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Role = dto.Role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            _logger.Log($"User registered: {dto.Username} ({dto.Role})");

            return Ok(new { Message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.Log($"Failed login attempt for: {dto.Username}");
                return Unauthorized(new { Message = "Invalid credentials." });
            }

            if (user.Role != dto.Role)
            {
                _logger.Log($"Role mismatch login attempt for: {dto.Username}");
                return Unauthorized(new { Message = "Role mismatch." });
            }

            _logger.Log($"User logged in: {user.Username}");

            return Ok(new LoginResponseDto
            {
                Username = user.Username,
                Role = user.Role,
                Message = "Login successful."
            });
        }
    }
}
