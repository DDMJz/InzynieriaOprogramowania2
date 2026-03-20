using System.ComponentModel.DataAnnotations;

namespace FleetManager.DTOs
{
    public class FuelingEventCreateDto
    {
        [Required]
        public int VehicleId { get; set; }
        [Required]
        public double LitersAdded { get; set; }
        [Required]
        public double TotalCost { get; set; }
        [Required]
        public int OdometerReading { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}
