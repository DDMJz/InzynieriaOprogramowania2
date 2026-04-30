using FleetManager.Models;

namespace FleetManager.Strategies
{
    public class DualIntervalStrategy : IMaintenanceStrategy
    {
        private const double SevereUsageFuelThreshold = 1.20; // 20% powyzej normy
        private const double IntervalReductionPenalty = 0.70; // skrócenie interwału do 70% bazy
        private const int WarningKilometerThreshold = 1000;   // ostrzezenie na 1000 km przed
        private const int WarningDaysThreshold = 14;          // ostrzezenie na 14 dni przed terminem

        public bool CanHandle(MaintenanceType maintenanceType)
        {
            // stratyegia aktywna wylacznie dla przegladow ktore posiadaja oba interwaly
            return maintenanceType.DefaultIntervalOdometer.HasValue &&
                   maintenanceType.DefaultIntervalDays.HasValue;
        }

        public MaintenanceEvaluationResult Evaluate(MaintenanceEvaluationContext context)
        {
            int defaultKm = context.MaintenanceType.DefaultIntervalOdometer.Value;
            int lastOdometer = context.LastMaintenanceEvent?.OdometerReading ?? 0;  // jesli samochod nie mial jeszcze tego typu przegladu liczymy od 0
            int distanceDriven = context.Vehicle.OdometerReading - lastOdometer;

            // detekcja nadmiernego spalania i redukcja interwalu km
            int effectiveKmInterval = defaultKm;
            bool isSevereUsage = false;

            if (context.Vehicle.FuelConsumption > 0 &&
                context.FuelConsumptionSinceLastMaintenance > (context.Vehicle.FuelConsumption * SevereUsageFuelThreshold))
            {
                effectiveKmInterval = (int)(defaultKm * IntervalReductionPenalty);
                isSevereUsage = true;
            }

            int kilometersRemaining = effectiveKmInterval - distanceDriven;

            int defaultDays = context.MaintenanceType.DefaultIntervalDays.Value;

            DateTime lastMaintenanceDate = context.LastMaintenanceEvent?.Date ?? context.Vehicle.CreatedAt;
            int daysPassed = (int)(DateTime.UtcNow - lastMaintenanceDate).TotalDays;

            // redukcja interwalu czasowego jezeli nadmierne spalanie
            int effectiveDaysInterval = isSevereUsage ? (int)(defaultDays * IntervalReductionPenalty) : defaultDays;
            int daysRemaining = effectiveDaysInterval - daysPassed;

            string severeUsageNote = isSevereUsage ? " (Spalanie powyżej normy - interwal skrocony.)" : "";
            string maintenanceName = context.MaintenanceType.Name;

            if (kilometersRemaining <= 0 || daysRemaining <= 0)
            {
                string reason = kilometersRemaining <= 0
                    ? $"Przekroczono limit kilometrów o {Math.Abs(kilometersRemaining)} km."
                    : $"Przekroczono limit czasu o {Math.Abs(daysRemaining)} dni.";

                return new MaintenanceEvaluationResult(
                    Level: MaintenanceStatusLevel.Critical,
                    Message: $"KRYTYCZNE [{maintenanceName}]: {reason}{severeUsageNote}",
                    KilometersRemaining: kilometersRemaining,
                    DaysRemaining: daysRemaining,
                    PredictedMaintenanceDate: null
                );
            }

            if (kilometersRemaining <= WarningKilometerThreshold || daysRemaining <= WarningDaysThreshold)
            {
                string reason = kilometersRemaining <= WarningKilometerThreshold
                    ? $"Pozostało {kilometersRemaining} km."
                    : $"Pozostało {daysRemaining} dni.";

                return new MaintenanceEvaluationResult(
                    Level: MaintenanceStatusLevel.Warning,
                    Message: $"OSTRZEŻENIE [{maintenanceName}]: Zbliża się przegląd. {reason}{severeUsageNote}",
                    KilometersRemaining: kilometersRemaining,
                    DaysRemaining: daysRemaining,
                    PredictedMaintenanceDate: DateTime.UtcNow.AddDays(daysRemaining)
                );
            }

            return new MaintenanceEvaluationResult(
                Level: MaintenanceStatusLevel.Ok,
                Message: $"OK [{maintenanceName}]: Status prawidłowy.{severeUsageNote}",
                KilometersRemaining: kilometersRemaining,
                DaysRemaining: daysRemaining,
                PredictedMaintenanceDate: DateTime.UtcNow.AddDays(daysRemaining)
            );
        }
    }
}