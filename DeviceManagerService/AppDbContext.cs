namespace DeviceManager
{
    using DataAccess.DTOs;
    using Entities;
    using Microsoft.EntityFrameworkCore;

    public sealed class AppDbContext : DbContext
    {
        public DbSet<DeviceManagerEvent> Events { get; set; } = null;
        public DbSet<TestResult> TestResults { get; set; } = null;
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("Data Source=DeviceManager.db");
    }
}