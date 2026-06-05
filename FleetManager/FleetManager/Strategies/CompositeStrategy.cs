using FleetManager.Models;

namespace FleetManager.Strategies
{
    public class CompositeStrategy : IMaintenanceStrategy
    {
        private readonly TimeOnlyStrategy _timeStrategy;
        private readonly DistanceOnlyStrategy _distanceStrategy;

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
            // wywolanie Evaluate() z celowym pominieciem sprawdzenia przez CanHandle() 
            var timeResult = _timeStrategy.Evaluate(context);
            var distanceResult = _distanceStrategy.Evaluate(context);

            // ustalany poziom ostateczny
            MaintenanceStatusLevel finalLevel = distanceResult.Level > timeResult.Level  ? distanceResult.Level : timeResult.Level;

            // ustalana wiadomosc - prog czasu uprzywilejowany jezeli oba na tym smym poziomie
            string finalMessage = finalLevel == timeResult.Level ? timeResult.Message : distanceResult.Message;
            
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