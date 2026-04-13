using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetManager.Models
{
    public class FuelingEvent
    {
        public int Id { get; set; }

        // klucz obcy 
        [Required]
        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        [Required]
        public int OdometerReading { get; set; }

        [Range(0.01, 1000.0)]
        public double LitersAdded { get; set; }

        [Range(0.01, 100000.0)]
        public double Cost { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;

        // ----------------optymistyczna wspolbieznosc
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
