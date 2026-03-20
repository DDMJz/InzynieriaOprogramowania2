namespace FleetManager.DTOs
{
    public class MaintenanceEventReadDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }

        public int MaintenanceTypeId { get; set; }
        public string MaintenanceTypeName { get; set; } = string.Empty;

        public int OdometerReading { get; set; }
        public double TotalCost { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
