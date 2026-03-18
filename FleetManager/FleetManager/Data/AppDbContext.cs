using Microsoft.EntityFrameworkCore; 
using FleetManager.Models;           // Dostęp do klasy Vehicle


namespace FleetManager.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        //reprezentacja tabel bazy danych:
        public DbSet<Vehicle> Vehicles { get; set; } 
        public DbSet<FuelingEvent> FuelingEvents { get; set; }
        public DbSet<MaintenanceEvent> MaintenanceEvents { get; set; }
        public DbSet<MaintenanceType> MaintenanceTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Tutaj mozliwa dodatkowa niestandadrowa konfiguracja

            // wymuszenie unikalnosci pole Vin w vehicle na poziomie bazy danych: 
            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.Vin)
                .IsUnique();
        }
    }
}
