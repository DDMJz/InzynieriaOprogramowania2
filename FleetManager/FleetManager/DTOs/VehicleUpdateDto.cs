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
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
