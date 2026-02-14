using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InternExcelTracker.Api.Data;
using InternExcelTracker.Api.Models;
using InternExcelTracker.Api.DTOs;

namespace InternExcelTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InternController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly Services.ILoggerService _logger;

        public InternController(ApplicationDbContext context, IWebHostEnvironment environment, Services.ILoggerService logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet("assignments/{username}")]
        public async Task<IActionResult> GetAssignments(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound(new { Message = "User not found." });

            var assignments = await _context.ExcelAssignments
                .Where(a => a.InternId == user.Id)
                .Select(a => new AssignmentDto
                {
                    AssignmentId = a.Id,
                    FileName = a.FileName,
                    Status = a.Status,
                    AssignedDate = a.AssignedDate
                })
                .ToListAsync();

            return Ok(assignments);
        }

        [HttpGet("download/{assignmentId}")]
        public async Task<IActionResult> DownloadAssignment(int assignmentId)
        {
            var assignment = await _context.ExcelAssignments.FindAsync(assignmentId);
            if (assignment == null) return NotFound(new { Message = "Assignment not found." });

            var uploadsPath = Path.Combine(_environment.ContentRootPath, "Uploads");
            // FilePath in DB was stored as relative filename in AdminController
            var filePath = Path.Combine(uploadsPath, assignment.FilePath);

            if (!System.IO.File.Exists(filePath))
                return NotFound(new { Message = "File on server not found." });

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", assignment.FileName);
        }

        [HttpPost("complete-assignment/{id}")]
        public async Task<IActionResult> CompleteAssignment(int id)
        {
            var assignment = await _context.ExcelAssignments.FindAsync(id);
            if (assignment == null) return NotFound(new { Message = "Assignment not found." });

            assignment.Status = "Completed";
            await _context.SaveChangesAsync();

            _logger.Log($"Assignment {id} marked as completed.");

            return Ok(new { Message = "Assignment marked as completed." });
        }

        [HttpPost("submit-report")]
        public async Task<IActionResult> SubmitReport(SubmitReportDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null) return BadRequest(new { Message = "User not found." });

            // Auto-increment logic: Count reports created today by this user
            var today = DateTime.UtcNow.Date;
            var dailyCount = await _context.ProductReports
                .Where(r => r.InternId == user.Id && r.CreatedAt.Date == today)
                .CountAsync();
            
            var newProductId = (dailyCount + 1).ToString();

            var report = new ProductReport
            {
                ExcelAssignmentId = dto.AssignmentId,
                InternId = user.Id,
                ProductId = newProductId,
                ProductName = dto.ProductName,
                ImagesCollected = dto.ImagesCollected,
                ImageQuality = dto.ImageQuality,
                VideoCollected = dto.VideoCollected,
                Remarks = dto.Remarks,
                ApprovalStatus = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.ProductReports.Add(report);
            await _context.SaveChangesAsync();

            _logger.Log($"Intern {dto.Username} submitted report for product {dto.ProductName} (ID: {newProductId})");

            return Ok(new { Message = "Report submitted successfully." });
        }

        [HttpGet("reports/{username}")]
        public async Task<IActionResult> GetMyReports(string username)
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

        [HttpPut("edit-report/{reportId}")]
        public async Task<IActionResult> EditReport(int reportId, [FromBody] SubmitReportDto dto)
        {
            var report = await _context.ProductReports.FindAsync(reportId);
            if (report == null) return NotFound(new { Message = "Report not found." });

            if (report.ApprovalStatus == "Approved")
                return BadRequest(new { Message = "Cannot edit approved reports." });

            // ProductId is auto-generated and should not be editable
            // report.ProductId = dto.ProductId; 
            report.ProductName = dto.ProductName;
            report.ImagesCollected = dto.ImagesCollected;
            report.ImageQuality = dto.ImageQuality;
            report.VideoCollected = dto.VideoCollected;
            report.Remarks = dto.Remarks;
            // Reset status on edit? Usually yes, asking for re-approval.
            report.ApprovalStatus = "Pending";

            await _context.SaveChangesAsync();

            _logger.Log($"Intern {dto.Username} edited report ID: {reportId}");

            return Ok(new { Message = "Report updated." });
        }

        [HttpDelete("delete-report/{reportId}")]
        public async Task<IActionResult> DeleteReport(int reportId)
        {
            var report = await _context.ProductReports.FindAsync(reportId);
            if (report == null) return NotFound(new { Message = "Report not found." });

            if (report.ApprovalStatus == "Approved")
                return BadRequest(new { Message = "Cannot delete approved reports." });

            _context.ProductReports.Remove(report);
            await _context.SaveChangesAsync();

            _logger.Log($"Intern deleted report ID: {reportId}");

            return Ok(new { Message = "Report deleted." });
        }
    }
}
