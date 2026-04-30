namespace FleetManager.DTOs
{
    public record MaintenanceTypeReadDto(
        int Id,
        string Name,
        string SystemCode,
        int? DefaultIntervalOdometer,
        int? DefaultIntervalDays
    );
}
