namespace FleetManager.DTOs
{
    public class FuelingEventReadDto
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }

        public int OdometerReading { get; set; }

        public double LitersAdded { get; set; }
        public double TotalCost { get; set; }

        public DateTime Date { get; set; }

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
