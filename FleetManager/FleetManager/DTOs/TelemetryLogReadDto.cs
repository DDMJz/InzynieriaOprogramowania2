namespace FleetManager.DTOs
{
    public class TelemetryLogReadDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? SpeedKph { get; set; }
        public double? FuelLevel { get; set; }
        public DateTime Timestamp { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
