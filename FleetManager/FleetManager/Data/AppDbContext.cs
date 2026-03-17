using Microsoft.EntityFrameworkCore; // Biblioteka obslugujaca baze danych
using FleetManager.Models;           // Dostęp do klasy Vehicle


namespace FleetManager.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; } //reprezentuje tabele z bazy danych
    }
}
