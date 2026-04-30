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

            if (daysRemaining <= 0)
                return new MaintenanceEvaluationResult(MaintenanceStatusLevel.Critical, $"KRYTYCZNE [{name}]: Przekroczono limit czasu o {Math.Abs(daysRemaining)} dni.", null, daysRemaining, null);

            if (daysRemaining <= WarningDaysThreshold)
                return new MaintenanceEvaluationResult(MaintenanceStatusLevel.Warning, $"OSTRZEŻENIE [{name}]: Zbliża się termin. Pozostało {daysRemaining} dni.", null, daysRemaining, DateTime.UtcNow.AddDays(daysRemaining));

            return new MaintenanceEvaluationResult(MaintenanceStatusLevel.Ok, $"OK [{name}]: Status prawidłowy.", null, daysRemaining, DateTime.UtcNow.AddDays(daysRemaining));
        }
    }
}