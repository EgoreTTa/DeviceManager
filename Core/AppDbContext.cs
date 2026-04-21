namespace Core
{
    using DriverBase.DTOs;
    using Entities;
    using Microsoft.EntityFrameworkCore;

    public sealed class AppDbContext : DbContext
    {
        public DbSet<DeviceManagerEvent> Events { get; set; }
        public DbSet<TestResultDTO> TestResults { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("Data Source=DeviceManager.db");
    }
}