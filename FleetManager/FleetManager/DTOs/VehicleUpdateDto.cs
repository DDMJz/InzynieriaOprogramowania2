namespace FleetManager.DTOs
{
    public class VehicleUpdateDto
    {
        public string LicensePlate { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
