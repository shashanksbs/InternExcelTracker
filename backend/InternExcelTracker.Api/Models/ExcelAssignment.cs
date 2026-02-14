using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternExcelTracker.Api.Models
{
    public class ExcelAssignment
    {
        public int Id { get; set; }

        [Required]
        public int InternId { get; set; }

        [ForeignKey("InternId")]
        public User? Intern { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending"; // Pending, Completed

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    }
}
