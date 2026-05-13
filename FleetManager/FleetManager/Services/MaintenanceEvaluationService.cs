using FleetManager.Models;
using FleetManager.Strategies;

namespace FleetManager.Services
{
    public class MaintenanceEvaluationService : IMaintenanceEvaluationService
    {
       // kolekcja wszystkich strategii implementujacych ten interfejs
        private readonly IEnumerable<IMaintenanceStrategy> _strategies;

        public MaintenanceEvaluationService(IEnumerable<IMaintenanceStrategy> strategies)
        {
            _strategies = strategies;
        }

        public MaintenanceEvaluationResult EvaluateVehicleMaintenance(
            Vehicle vehicle,
            MaintenanceType maintenanceType,
            MaintenanceEvent? lastEvent,
            double fuelConsumedSinceLast)
        {
            var context = new MaintenanceEvaluationContext(
                Vehicle: vehicle,
                MaintenanceType: maintenanceType,
                LastMaintenanceEvent: lastEvent,
                FuelConsumptionSinceLastMaintenance: fuelConsumedSinceLast,
                EffectiveIntervalOdometer: maintenanceType.DefaultIntervalOdometer,
                EffectiveIntervalDays: maintenanceType.DefaultIntervalDays
            );

            // typowanie strategi ktora przetworzy ten MaintenanceType
            var strategy = _strategies.FirstOrDefault(s => s.CanHandle(maintenanceType));

            if (strategy == null)
            {
                throw new NotSupportedException($"KRYTYCZNE: Brak zarejestrowanej strategii obsługującej typ przeglądu: {maintenanceType.SystemCode}");
            }

            return strategy.Evaluate(context);
        }
    }
}
