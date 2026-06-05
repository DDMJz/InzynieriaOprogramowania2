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
            var baseContext = new MaintenanceEvaluationContext(
                Vehicle: vehicle,
                MaintenanceType: maintenanceType,
                LastMaintenanceEvent: lastEvent,
                FuelConsumptionSinceLastMaintenance: fuelConsumedSinceLast,
                EffectiveIntervalOdometer: maintenanceType.DefaultIntervalOdometer,
                EffectiveIntervalDays: maintenanceType.DefaultIntervalDays
            );

            //  zmniejszanie interwalów domyslnych w razie spalania nadmiernego 
            var finalContext = ApplySevereUsagePenalty(baseContext);

            // typowanie strategii ktora przetworzy ten MaintenanceType
            var strategy = _strategies.FirstOrDefault(s => s.CanHandle(maintenanceType));

            if (strategy == null)
            {
                throw new NotSupportedException($"KRYTYCZNE: Brak zarejestrowanej strategii obsługującej typ przeglądu: {maintenanceType.SystemCode}");
            }

            return strategy.Evaluate(finalContext);
        }

        private MaintenanceEvaluationContext ApplySevereUsagePenalty(MaintenanceEvaluationContext context)
        {
            const double SevereUsageFuelThreshold = 1.20;   // prog spalania - 120%
            const double IntervalReductionPenalty = 0.70;   // redukcja interwłow - 70%

            if (context.Vehicle.FuelConsumption <= 0)
            {
                //throw new InvalidOperationException($"KRYTYCZNE: Wartość zakładanego spalania w bazie dla pojazdu o ID {context.Vehicle.Id} jest niemożliwa: {context.Vehicle.FuelConsumption}");
            }

            if (context.FuelConsumptionSinceLastMaintenance < (context.Vehicle.FuelConsumption * SevereUsageFuelThreshold))
            {
                return context; // jezeli spalanie nie przekracza 120% normy
            }

            int? newKmInterval = context.EffectiveIntervalOdometer;
            int? newDaysInterval = context.EffectiveIntervalDays;

            if (newKmInterval.HasValue) newKmInterval = (int)(newKmInterval.Value * IntervalReductionPenalty);
            if (newDaysInterval.HasValue) newDaysInterval = (int)(newDaysInterval.Value * IntervalReductionPenalty);

            // mutowanie niemutowalnego obiektu typu record
            return context with
            {
                EffectiveIntervalOdometer = newKmInterval,
                EffectiveIntervalDays = newDaysInterval,
                penaltyApplied = true
            };
        }

    }
}
