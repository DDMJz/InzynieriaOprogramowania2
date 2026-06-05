using System.ComponentModel.DataAnnotations;

namespace FleetManager.DTOs
{
    public class VehicleReadDto
    {
        public int Id { get; set; }
        public string Vin { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public int OdometerReading { get; set; }
        public double FuelTankCapacity { get; set; }
        public double FuelConsumption { get; set; }
        public string Status { get; set; } = string.Empty;
        public byte[] RowVersion { get; set; } = Array.Empty<byte>(); //potrzebne dla VehicleUpdateDto
    }
}
