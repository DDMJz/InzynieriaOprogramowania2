namespace FleetManager.DTOs
{
    public class FuelStatisticsDto
    {
        public int VehicleId { get; set; }
        public int TotalDistanceKm { get; set; }
        public double TotalFuelLiters { get; set; }
        public double TotalCost { get; set; }
        public double AverageConsumption { get; set; }
    }
}
