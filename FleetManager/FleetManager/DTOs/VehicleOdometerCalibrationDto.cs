using System.ComponentModel.DataAnnotations;

namespace FleetManager.DTOs
{
    public class VehicleOdometerCalibrationDto
    {
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Przebieg nie może byc ujemny.")]
        public int NewOdometerReading { get; set; }

        [Required]
        [MinLength(5, ErrorMessage = "Nalezy podac powod kalibracji.")]
        public string Justification { get; set; } = string.Empty;
    }
}
