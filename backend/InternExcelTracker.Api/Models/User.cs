using System;
using System.ComponentModel.DataAnnotations;

namespace InternExcelTracker.Api.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Intern"; // Admin / Intern

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
