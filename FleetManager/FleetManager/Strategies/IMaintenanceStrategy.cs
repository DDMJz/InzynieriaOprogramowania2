using FleetManager.Models;

namespace FleetManager.Strategies
{
    public interface IMaintenanceStrategy
    {
        // sprawdzenie czy dany typ przeglądu 
        bool CanHandle(MaintenanceType maintenanceType);

        // wykonanie strategii
        MaintenanceEvaluationResult Evaluate(MaintenanceEvaluationContext context);
    }
}