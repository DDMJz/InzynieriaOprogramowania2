using System.ComponentModel.DataAnnotations;

namespace FleetManager.Models
{
    public class MaintenanceType
    {
        // Klucz infrastrukturalny (Primary Key) - dla bazy danych i relacji SQL
        public int Id { get; set; }

        // Klucz domenowy (Business Key) -  dla strategii
        [Required, MaxLength(50)]
        public string SystemCode { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int? DefaultIntervalOdometer { get; set; } // np. 15000 (Olej) lub null (gasnica)
        public int? DefaultIntervalDays { get; set; }     // np. 365 (gasnica) lub null (klocki hamulcowe)
    }
}