using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternExcelTracker.Api.Models
{
    public class ProductReport
    {
        public int Id { get; set; }

        [Required]
        public int ExcelAssignmentId { get; set; }

        [ForeignKey("ExcelAssignmentId")]
        public ExcelAssignment? ExcelAssignment { get; set; }
        
        [Required]
        public int InternId { get; set; }

        [ForeignKey("InternId")]
        public User? Intern { get; set; }

        [Required]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        public string ProductName { get; set; } = string.Empty;

        public int ImagesCollected { get; set; }
        public string ImageQuality { get; set; } = string.Empty;
        public bool VideoCollected { get; set; }
        public string Remarks { get; set; } = string.Empty;

        public string ApprovalStatus { get; set; } = "Pending"; // Pending/Approved/Rejected
        public string RejectionReason { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
