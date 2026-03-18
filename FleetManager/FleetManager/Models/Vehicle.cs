using System.ComponentModel.DataAnnotations;

namespace FleetManager.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string Vin { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; //wazne - niezaleznosc od czasu lokalnego
        
        
        //------------------modul tankowania 
        public double FuelTankCapacity { get; set; }
        public double CurrentFuelLevel { get; set; }
        // Kalkulator procentowy (liczy C#, nie baza)
        public double FuelLevelPercentage => FuelTankCapacity > 0
            ? Math.Round((CurrentFuelLevel / FuelTankCapacity) * 100, 2)
            : 0;

        //------------------modul przebiegow i eksploatacji bedzie zrealizowany odzielnej klasie z logika  
        public double OdometerReading { get; set; }
        

        // ----------modul GPS (wazne -typy nullable)
        public double? LastKnownLatitude { get; set; }
        public double? LastKnownLongitude { get; set; }
        public DateTime? LastGpsUpdate { get; set; }

        // ----------------optymistyczna wspolbieznosc
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // ---relacje jeden-do-wielu
        public ICollection<FuelingEvent> FuelingEvents { get; set; } = new List<FuelingEvent>();
        public ICollection<MaintenanceEvent> MaintenanceEvents { get; set; } = new List<MaintenanceEvent>();
    }
}
