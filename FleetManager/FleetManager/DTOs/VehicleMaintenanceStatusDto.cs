namespace FleetManager.DTOs
{
    public record VehicleMaintenanceStatusDto(
        int MaintenanceTypeId,        
        string MaintenanceTypeName,
        string StatusLevel,
        string Message,
        int? KilometersRemaining,
        int? DaysRemaining,
        DateTime? PredictedMaintenanceDate
    );
}