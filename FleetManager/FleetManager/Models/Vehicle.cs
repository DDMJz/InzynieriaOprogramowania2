namespace FleetManager.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string Vin { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public double OdometerReading { get; set; }
        public double FuelTankCapacity { get; set; }
        public double CurrentFuelLevel { get; set; } // NOWE POLE
        public int Year { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Kalkulator procentowy (liczy C#, nie baza)
        public double FuelLevelPercentage => FuelTankCapacity > 0
            ? Math.Round((CurrentFuelLevel / FuelTankCapacity) * 100, 2)
            : 0;
    }
}
