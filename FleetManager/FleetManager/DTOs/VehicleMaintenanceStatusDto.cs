namespace FleetManager.DTOs
{
    public record VehicleMaintenanceStatusDto(
        string StatusLevel,
        string Message,
        int? KilometersRemaining,
        int? DaysRemaining,
        DateTime? PredictedMaintenanceDate
    );
}