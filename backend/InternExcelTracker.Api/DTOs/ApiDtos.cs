using System;
using System.ComponentModel.DataAnnotations;

namespace InternExcelTracker.Api.DTOs
{
    // Auth DTOs
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = "Intern"; // Admin / Intern
    }

    public class LoginDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
        
        public string Role { get; set; } = "Intern";
    }

    public class LoginResponseDto
    {
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    // Admin DTOs
    public class UploadExcelDto
    {
        [Required]
        public IFormFile File { get; set; }
        public string UploadedByUsername { get; set; } = string.Empty;
        public string AssignedToUsername { get; set; } = string.Empty;
    }

    public class PerformanceStatsDto
    {
        public int TotalProducts { get; set; }
        public int Completed { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
    }

    // Intern DTOs
    public class AssignmentDto
    {
        public int AssignmentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
    }

    public class SubmitReportDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public int AssignmentId { get; set; }
        
        public string ProductId { get; set; } = string.Empty;
        
        [Required]
        public string ProductName { get; set; } = string.Empty;
        
        public int ImagesCollected { get; set; }
        public string ImageQuality { get; set; } = string.Empty;
        public bool VideoCollected { get; set; }
        public string? Remarks { get; set; } = string.Empty;
    }
}
