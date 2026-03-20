using System.ComponentModel.DataAnnotations;

namespace FleetManager.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string Vin { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; //wazne - niezaleznosc od czasu lokalnego
        
        
        //------------------modul tankowania 
        public double FuelTankCapacity { get; set; }
        public double CurrentFuelLevel { get; set; }
        // Kalkulator procentowy (liczy C#, nie baza)
        public double FuelLevelPercentage => FuelTankCapacity > 0
            ? Math.Round((CurrentFuelLevel / FuelTankCapacity) * 100, 2)
            : 0;

        //------------------modul przebiegow i eksploatacji bedzie zrealizowany odzielnej klasie z logika  
        public int OdometerReading { get; set; }
        //w tej chwili licznik jest aktualizowany jednynie podczas zdarzen tj fuelingEvent i maintenanceEvent
        //docelowo nalezy zrobic dodatkowy endpoint dla http patch od urzedzenia iot w samochodzie?
        //albo zastosowac protokol MQTT -  urzedzenie iot wysyla dane tym protokolem do brokera, a ASP.NET Core jest subskrybentem brokera
        // urzadzenie iot nie ma narzutu bo MQTT jest duzo lzejszy niz http?

        // ----------modul GPS (wazne -typy nullable)
        public double? LastKnownLatitude { get; set; }
        public double? LastKnownLongitude { get; set; }
        public DateTime? LastGpsUpdate { get; set; }

        // ----------------optymistyczna wspolbieznosc
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // ---listy dla relacji jeden-do-wielu
        public ICollection<FuelingEvent> FuelingEvents { get; set; } = new List<FuelingEvent>();
        public ICollection<MaintenanceEvent> MaintenanceEvents { get; set; } = new List<MaintenanceEvent>();
    }
}
