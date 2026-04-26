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
            // Dalej mozliwa dodatkowa konfiguracja:

            
            modelBuilder.Entity<Vehicle>(v =>
            {
                v.HasIndex(x => x.Vin).IsUnique(); // wymuszenie unikalnoscinikalność
                v.Property(x => x.Vin).HasMaxLength(17).IsFixedLength(); // wymuszenie długości vin
                v.Property(x => x.LicensePlate).HasMaxLength(10).IsRequired(); //max dlugosc rejestracji
            });

            modelBuilder.Entity<FuelingEvent>()
                .HasOne(f => f.Vehicle)
                .WithMany(v => v.FuelingEvents) //vehicle ma liste tankowan(relacja jeden do wielu dwukierunkowa)
                .HasForeignKey(f => f.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MaintenanceEvent>()
                .HasOne(m => m.Vehicle)
                .WithMany(v => v.MaintenanceEvents) //vehicle ma liste napraw(relacja jeden do wielu dwukierunkowa)
                .HasForeignKey(m => m.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MaintenanceEvent>()
                .HasOne(m => m.MaintenanceType)
                .WithMany() // Typ nie ma listy napraw (realcja jeden do wielu ale jednokierunkowa)
                .HasForeignKey(m => m.MaintenanceTypeId)
                .OnDelete(DeleteBehavior.Restrict); //niedomyslen ustawienie - ma chronic historie napraw przy usunieciu typu naprawy

            //seedowanie domyslnych typow napraw do bazy: 
            modelBuilder.Entity<MaintenanceType>().HasData(
                new MaintenanceType
                {
                    Id = 1,
                    Name = "Wymiana Oleju",
                    DefaultIntervalOdometer = 15000,
                    DefaultIntervalDays = 365
                },
                new MaintenanceType
                {
                    Id = 2,
                    Name = "Przegląd Rejestracyjny",
                    DefaultIntervalOdometer = 0,
                    DefaultIntervalDays = 365
                },
                new MaintenanceType
                {
                    Id = 99, 
                    Name = "Inne",
                    DefaultIntervalOdometer = 0,
                    DefaultIntervalDays = 0
                }
            );
        }
    }
}
