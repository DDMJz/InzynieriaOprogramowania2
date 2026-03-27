using System.ComponentModel.DataAnnotations;

namespace FleetManager.DTOs
{
    public class TelemetryLogCreateDto
    {
        [Required]
        public int VehicleId { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public double? SpeedKph { get; set; }
        public double? FuelLevel { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
