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
            MaintenanceStatusLevel finalLevel;
            string finalMessage;

            if (kilometersRemaining <= 0)
            {
                finalLevel = MaintenanceStatusLevel.Critical;
                finalMessage = $"KRYTYCZNE [{name}]: Przekroczono limit o {Math.Abs(kilometersRemaining)} km.";
            }
            else if (kilometersRemaining <= WarningKilometerThreshold)
            {
                finalLevel = MaintenanceStatusLevel.Warning;
                finalMessage = $"OSTRZEŻENIE [{name}]: Zbliża się wymiana. Pozostało {kilometersRemaining} km.";
            }
            else
            {
                finalLevel = MaintenanceStatusLevel.Ok;
                finalMessage = $"OK [{name}]: Status prawidłowy.";
            }

            if (context.penaltyApplied)
            {
                finalMessage += " (Spalanie powyżej normy - interwał skrócony.)";
            }

            return new MaintenanceEvaluationResult(
                Level: finalLevel,
                Message: finalMessage,
                KilometersRemaining: kilometersRemaining,
                DaysRemaining: null,
                PredictedMaintenanceDate: null
            );
        }
    }
}