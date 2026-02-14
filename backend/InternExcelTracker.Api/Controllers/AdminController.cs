using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InternExcelTracker.Api.Data;
using InternExcelTracker.Api.Models;
using InternExcelTracker.Api.DTOs;

namespace InternExcelTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly Services.ILoggerService _logger;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment environment, Services.ILoggerService logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        [HttpPost("upload-excel")]
        public async Task<IActionResult> UploadExcel([FromForm] UploadExcelDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest(new { Message = "No file uploaded." });

            var intern = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.AssignedToUsername && u.Role == "Intern");
            if (intern == null)
                return BadRequest(new { Message = "Intern not found." });

            // Ensure uploads folder exists
            var uploadsPath = Path.Combine(_environment.ContentRootPath, "Uploads");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var assignment = new ExcelAssignment
            {
                InternId = intern.Id,
                FileName = dto.File.FileName,
                FilePath = fileName, // Storing relative filename for simplicity
                AssignedDate = DateTime.UtcNow
            };

            _context.ExcelAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            _logger.Log($"Admin assigned {fileName} to {intern.Username}");

            return Ok(new { Message = "Excel assigned successfully." });
        }

        [HttpGet("reports")]
        public async Task<IActionResult> GetReports(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound(new { Message = "User not found" });

            var reports = await _context.ProductReports
                .Include(r => r.ExcelAssignment)
                .Where(r => r.InternId == user.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(reports);
        }

        [HttpPut("approve/{reportId}")]
        public async Task<IActionResult> ApproveReport(int reportId)
        {
            var report = await _context.ProductReports.FindAsync(reportId);
            if (report == null) return NotFound("Report not found.");

            report.ApprovalStatus = "Approved";
            await _context.SaveChangesAsync();

            _logger.Log($"Admin approved report ID: {reportId}");

            return Ok(new { Message = "Report approved." });
        }

        [HttpPut("reject/{reportId}")]
        public async Task<IActionResult> RejectReport(int reportId, [FromBody] RejectDto dto)
        {
            var report = await _context.ProductReports.FindAsync(reportId);
            if (report == null) return NotFound(new { Message = "Report not found." });

            report.ApprovalStatus = "Rejected";
            report.RejectionReason = dto.Reason;
            await _context.SaveChangesAsync();

            _logger.Log($"Admin rejected report ID: {reportId}. Reason: {dto.Reason}");

            return Ok(new { Message = "Report rejected." });
        }

        [HttpGet("performance")]
        public async Task<IActionResult> GetPerformance(string username, DateTime? date)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound(new { Message = "User not found" });

            var query = _context.ProductReports.AsQueryable()
                .Where(r => r.InternId == user.Id);

            if (date.HasValue)
            {
                // Comparing logic for date (ignoring time)
                var nextDay = date.Value.AddDays(1);
                query = query.Where(r => r.CreatedAt >= date.Value && r.CreatedAt < nextDay);
            }

            var reports = await query.ToListAsync();

            var stats = new PerformanceStatsDto
            {
                TotalProducts = reports.Count,
                Completed = reports.Count(r => r.ApprovalStatus == "Approved"), // Assuming 'approved' counts as completed for now, or just all submitted
                Pending = reports.Count(r => r.ApprovalStatus == "Pending"),
                Approved = reports.Count(r => r.ApprovalStatus == "Approved"),
                Rejected = reports.Count(r => r.ApprovalStatus == "Rejected")
            };
            
            // Note: 'Completed' might mean something else, essentially total submitted is what we have here.
            stats.Completed = stats.Approved + stats.Rejected + stats.Pending; // Or just reports.Count

            return Ok(stats);
        }

        // Helper to get all interns for dropdowns
        [HttpGet("interns")]
        public async Task<IActionResult> GetAllInterns()
        {
            var interns = await _context.Users
                .Where(u => u.Role == "Intern")
                .Select(u => new { u.Username, u.Email })
                .ToListAsync();
            return Ok(interns);
        }
    }
}
