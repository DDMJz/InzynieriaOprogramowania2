using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetManager.Models
{
    public class MaintenanceEvent
    {
        // klasa reprezentuje wizyte w warsztacie  
        public int Id { get; set; }
        //Powiazanie z tabela vehicles:
        //klucz obcy
        public int VehicleId { get; set; }

        // Właściwość nawigacyjna i referencja obiektowa dla Entity Framework
        [ForeignKey(nameof(VehicleId))]
        public Vehicle? Vehicle { get; set; }

        // powiazanie z tabela typow napraw: 
        public int MaintenanceTypeId { get; set; }
        [ForeignKey(nameof(MaintenanceTypeId))]
        public MaintenanceType? MaintenanceType { get; set; }


        public string Description { get; set; } = string.Empty;
        public double OdometerAtService { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;

        
        // ----------------optymistyczna wspolbieznosc
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
