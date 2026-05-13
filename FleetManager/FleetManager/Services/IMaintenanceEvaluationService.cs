using FleetManager.Models;
using FleetManager.Strategies;

namespace FleetManager.Services
{
    public interface IMaintenanceEvaluationService
    {
        MaintenanceEvaluationResult EvaluateVehicleMaintenance(
            Vehicle vehicle,
            MaintenanceType maintenanceType,
            MaintenanceEvent? lastEvent,
            double fuelConsumedSinceLast);
    }
}
