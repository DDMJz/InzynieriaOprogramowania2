using System.ComponentModel.DataAnnotations;

namespace FleetManager.DTOs
{
    public class VehicleCreateDto
    {
        [Required]
        [StringLength(17, MinimumLength = 17)]
        public string Vin { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string LicensePlate { get; set; } = string.Empty;

        [Required]
        public string Brand { get; set; } = string.Empty;

        [Required]
        public string Model { get; set; } = string.Empty;
       
        [Range(0, int.MaxValue, ErrorMessage = "Przebieg nie może być ujemny.")]
        public int OdometerReading { get; set; }
    }
}
