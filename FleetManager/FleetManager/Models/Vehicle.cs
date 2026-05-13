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

        public VehicleStatus Status { get; set; } = VehicleStatus.Idle; //enum w tej samej przestrzeni nazw

        //------------------modul tankowania 
        public double FuelTankCapacity { get; set; }

        public double FuelConsumption { get; set; }


        //------------------modul przebiegow i eksploatacji bedzie zrealizowany odzielnej klasie z logika  
        public int OdometerReading { get; set; }


        // ----------------optymistyczna wspolbieznosc
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // ---listy dla relacji jeden-do-wielu
        public ICollection<FuelingEvent> FuelingEvents { get; set; } = new List<FuelingEvent>();
        public ICollection<MaintenanceEvent> MaintenanceEvents { get; set; } = new List<MaintenanceEvent>();
    }
}
