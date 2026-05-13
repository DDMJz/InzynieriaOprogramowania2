using FleetManager.Models;

namespace FleetManager.Strategies
{
    public class CompositeStrategy : IMaintenanceStrategy
    {
        private readonly TimeOnlyStrategy _timeStrategy;
        private readonly DistanceOnlyStrategy _distanceStrategy;

        private const double SevereUsageFuelThreshold = 1.20; // prog spalania - 120%
        private const double IntervalReductionPenalty = 0.70; // redukcja interwłow - 70%

        // budowanie kompozytu z podwezlow
        public CompositeStrategy(TimeOnlyStrategy timeStrategy, DistanceOnlyStrategy distanceStrategy)
        {
            _timeStrategy = timeStrategy;
            _distanceStrategy = distanceStrategy;
        }

        public bool CanHandle(MaintenanceType maintenanceType)
        {
            // omze oblsuzyc tylko typy z jednoczesnym interwalem czasowym i dystansowym
            return maintenanceType.DefaultIntervalOdometer.HasValue &&
                   maintenanceType.DefaultIntervalDays.HasValue;
        }

        public MaintenanceEvaluationResult Evaluate(MaintenanceEvaluationContext context)
        {
            bool isSevereUsage = false;
            int? newKmInterval = context.EffectiveIntervalOdometer;
            int? newDaysInterval = context.EffectiveIntervalDays;

            // sprawdzanie spalania od odtaniego zdarzenia tego typu - jezeli ponad 120% fabrycznego redukcja interwalow
            if (context.Vehicle.FuelConsumption > 0 &&
                context.FuelConsumptionSinceLastMaintenance > (context.Vehicle.FuelConsumption * SevereUsageFuelThreshold))
            {
                isSevereUsage = true;
                if (newKmInterval.HasValue)
                {
                    newKmInterval = (int)(newKmInterval.Value * IntervalReductionPenalty);
                }
                if (newDaysInterval.HasValue)
                {
                    newDaysInterval = (int)(newDaysInterval.Value * IntervalReductionPenalty);
                }
            }

            // tutaj klonowanie niemutowalnego rekordu
            var mutatedContext = context with
            {
                EffectiveIntervalOdometer = newKmInterval,
                EffectiveIntervalDays = newDaysInterval
            };

            // wywolanie Evaluate() z celowym pominieciem sprawdzenia przez CanHandle() 
            var timeResult = _timeStrategy.Evaluate(mutatedContext);
            var distanceResult = _distanceStrategy.Evaluate(mutatedContext);

            // ustalany poziom ostateczny
            MaintenanceStatusLevel finalLevel = distanceResult.Level > timeResult.Level  ? distanceResult.Level : timeResult.Level;

            // ustalana wiadomosc - prog czasu uprzywilejowany jezeli oba na tym smym poziomie
            string finalMessage = finalLevel == timeResult.Level ? timeResult.Message : distanceResult.Message;
            if (isSevereUsage)
            {
                finalMessage += " (Spalanie powyżej normy - interwał skrócony.)";
            }

            return new MaintenanceEvaluationResult(
                Level: finalLevel,
                Message: finalMessage,
                KilometersRemaining: distanceResult.KilometersRemaining,
                DaysRemaining: timeResult.DaysRemaining,
                PredictedMaintenanceDate: timeResult.PredictedMaintenanceDate
            );
        }
    }
}