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
        public double CurrentFuelLevel { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>(); //potrzebne dla VehicleUpdateDto
    }
}
