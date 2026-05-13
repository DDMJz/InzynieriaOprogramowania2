using FleetManager.Models;
using System;

namespace FleetManager.Strategies
{
    public class DistanceOnlyStrategy : IMaintenanceStrategy
    {
        private const int WarningKilometerThreshold = 1000;

        public bool CanHandle(MaintenanceType maintenanceType)
        {
            // Odbiera tylko ładunki czysto kilometrowe (np. klocki hamulcowe)
            return maintenanceType.DefaultIntervalOdometer.HasValue &&
                   !maintenanceType.DefaultIntervalDays.HasValue;
        }

        public MaintenanceEvaluationResult Evaluate(MaintenanceEvaluationContext context)
        {
            if (!context.EffectiveIntervalOdometer.HasValue)
                throw new InvalidOperationException("KRYTYCZNE: Strategia dystansowa wywołana bez interwału przebiegu w ładunku wejściowym."); //celowo nieprzechwycony wyjatek zakonczy watek tego klienta ktory do tego doprowadzil

            int defaultKm = context.EffectiveIntervalOdometer.Value;
            int lastOdometer = context.LastMaintenanceEvent?.OdometerReading ?? 0;
            int distanceDriven = context.Vehicle.OdometerReading - lastOdometer;
            int kilometersRemaining = defaultKm - distanceDriven;

            string name = context.MaintenanceType.Name;

            if (kilometersRemaining <= 0)
                return new MaintenanceEvaluationResult(MaintenanceStatusLevel.Critical, $"KRYTYCZNE [{name}]: Przekroczono limit o {Math.Abs(kilometersRemaining)} km.", kilometersRemaining, null, null);

            if (kilometersRemaining <= WarningKilometerThreshold)
                return new MaintenanceEvaluationResult(MaintenanceStatusLevel.Warning, $"OSTRZEŻENIE [{name}]: Zbliża się wymiana. Pozostało {kilometersRemaining} km.", kilometersRemaining, null, null);

            return new MaintenanceEvaluationResult(MaintenanceStatusLevel.Ok, $"OK [{name}]: Status prawidłowy.", kilometersRemaining, null, null);
        }
    }
}