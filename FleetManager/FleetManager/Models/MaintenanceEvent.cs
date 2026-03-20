using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetManager.Models
{
    public class MaintenanceEvent
    {
        // klasa reprezentuje wizyte w warsztacie  
        public int Id { get; set; }

        // klucz obcy 
        [Required]
        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }


        // klucz obcy 
        [Required]
        public int MaintenanceTypeId { get; set; }
        public MaintenanceType? MaintenanceType { get; set; }

        [Required]
        public int OdometerReading { get; set; }

        public string Description { get; set; } = string.Empty;

        [Range(0.01, 100000.0)]
        public double TotalCost { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;

        
        // ----------------optymistyczna wspolbieznosc
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
