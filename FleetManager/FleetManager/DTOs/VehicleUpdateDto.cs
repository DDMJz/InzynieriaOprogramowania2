using System.ComponentModel.DataAnnotations;

namespace FleetManager.DTOs
{
    public class VehicleUpdateDto
    {
        [Required]
        public string LicensePlate { get; set; } = string.Empty;
        [Required]
        public string Brand { get; set; } = string.Empty;
        [Required]
        public string Model { get; set; } = string.Empty;
        [Required]
        public int Year { get; set; }
        [Required]
        [Range(1.0, 1000.0)]
        public double FuelTankCapacity { get; set; }
        [Required]
        [Range(1.0, 100.0)]
        public double FuelConsumption { get; set; }
        [Required]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
