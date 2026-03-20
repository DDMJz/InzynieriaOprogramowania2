using System.ComponentModel.DataAnnotations;

namespace FleetManager.DTOs
{
    public class MaintenanceEventCreateDto
    {
        [Required]
        public int VehicleId { get; set; }

        [Required]
        public int MaintenanceTypeId { get; set; }

        [Required]
        [Range(0, 2000000)]
        public int OdometerReading { get; set; }

        [Required]
        [Range(0.01, 100000.0)]
        public double TotalCost { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Opis nie może przekraczać 500 znaków.")]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}
