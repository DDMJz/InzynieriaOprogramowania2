using FleetManager.Models;
using System;

namespace FleetManager.Strategies
{
    public class TimeOnlyStrategy : IMaintenanceStrategy
    {
        private const int WarningDaysThreshold = 14;

        public bool CanHandle(MaintenanceType maintenanceType)
        {
            return maintenanceType.DefaultIntervalDays.HasValue &&
                   !maintenanceType.DefaultIntervalOdometer.HasValue;
        }

        public MaintenanceEvaluationResult Evaluate(MaintenanceEvaluationContext context)
        {
            if (!context.EffectiveIntervalDays.HasValue)
                throw new InvalidOperationException("KRYTYCZNE: Strategia dystansowa wywołana bez interwału czasu w ładunku wejściowym."); 

            int defaultDays = context.EffectiveIntervalDays.Value;
            DateTime lastDate = context.LastMaintenanceEvent?.Date ?? context.Vehicle.CreatedAt;
            int daysPassed = (int)(DateTime.UtcNow - lastDate).TotalDays;
            int daysRemaining = defaultDays - daysPassed;

            string name = context.MaintenanceType.Name;
            MaintenanceStatusLevel finalLevel;
            string finalMessage;
            DateTime predictedDate = DateTime.UtcNow.AddDays(daysRemaining);

            if (daysRemaining <= 0)
            {
                finalLevel = MaintenanceStatusLevel.Critical;
                finalMessage = $"KRYTYCZNE [{name}]: Przekroczono limit czasu o {Math.Abs(daysRemaining)} dni.";
                predictedDate = default; // Brak predykcji, wymiana natychmiastowa
            }
            else if (daysRemaining <= WarningDaysThreshold)
            {
                finalLevel = MaintenanceStatusLevel.Warning;
                finalMessage = $"OSTRZEŻENIE [{name}]: Zbliża się termin. Pozostało {daysRemaining} dni.";
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
                KilometersRemaining: null,
                DaysRemaining: daysRemaining,
                PredictedMaintenanceDate: daysRemaining <= 0 ? null : predictedDate
            );
                       
        }
    }
}