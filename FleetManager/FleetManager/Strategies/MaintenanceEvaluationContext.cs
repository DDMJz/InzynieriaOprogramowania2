using FleetManager.Models;

namespace FleetManager.Strategies
{
    //zastosowanie niemutowalnego record zamiast mutowalnego class
    public record MaintenanceEvaluationContext(
        Vehicle Vehicle,
        MaintenanceType MaintenanceType,
        MaintenanceEvent? LastMaintenanceEvent,
        double FuelConsumptionSinceLastMaintenance,

        int? EffectiveIntervalOdometer,
        int? EffectiveIntervalDays
    );
}