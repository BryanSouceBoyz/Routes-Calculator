using Microsoft.EntityFrameworkCore;
using RoutesCalculator.Infrastructure.Entities;

namespace RoutesCalculator.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TripLog> TripLogs => Set<TripLog>();
    }
}
