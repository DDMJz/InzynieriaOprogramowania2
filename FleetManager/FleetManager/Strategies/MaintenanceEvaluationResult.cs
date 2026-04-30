namespace FleetManager.Strategies
{
    public record MaintenanceEvaluationResult(
        MaintenanceStatusLevel Level,
        string Message,
        int? KilometersRemaining,
        int? DaysRemaining,
        DateTime? PredictedMaintenanceDate
    );
}