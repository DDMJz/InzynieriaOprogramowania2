using System.ComponentModel.DataAnnotations;

namespace FleetManager.Models
{
    // Prosty model logów telemetrycznych (GPS, prędkość, paliwo)
    public class TelemetryLog
    {
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? SpeedKph { get; set; }
        public double? FuelLevel { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
