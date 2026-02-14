using Microsoft.EntityFrameworkCore;
using InternExcelTracker.Api.Models;

namespace InternExcelTracker.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ExcelAssignment> ExcelAssignments { get; set; }
        public DbSet<ProductReport> ProductReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Additional configurations if needed
        }
    }
}
