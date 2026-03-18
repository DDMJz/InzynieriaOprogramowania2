using System.ComponentModel.DataAnnotations;

namespace FleetManager.Models
{
    public class MaintenanceType
    {
        //klasa reprezentuje rodzaj naprawy
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty; // np. "Wymiana Oleju"

        public double DefaultIntervalOdometer { get; set; } // np. 15000km
        public int DefaultIntervalDays { get; set; }      // np. 365

        // relacja jeden-do-wielu
        public ICollection<MaintenanceEvent> MaintenanceRecords { get; set; } = new List<MaintenanceEvent>();
    }
}
