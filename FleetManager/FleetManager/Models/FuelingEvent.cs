using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetManager.Models
{
    public class FuelingEvent
    {
        public int Id { get; set; }

        // klucz obcy 
        public int VehicleId { get; set; }

        // referencja obiektowa dla Entity Framework
        [ForeignKey(nameof(VehicleId))]
        public Vehicle? Vehicle { get; set; }

        
        public double LitersAdded { get; set; }
        public double TotalCost { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;

        // ----------------optymistyczna wspolbieznosc
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
