namespace DeviceManager
{
    using Entities;
    using Microsoft.EntityFrameworkCore;

    public sealed class AppDbContext : DbContext
    {
        public DbSet<DeviceManagerEvent> Events { get; set; } = null;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("Data Source=DeviceManager.db");
    }
}