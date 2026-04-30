using FleetManager.Data;
using FleetManager.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FleetManager.Services
{
    public class VehicleMaintenanceStatusService : IVehicleMaintenanceStatusService
    {
        private readonly AppDbContext _context;
        private readonly IMaintenanceEvaluationService _evaluationService;

        public VehicleMaintenanceStatusService(AppDbContext context, IMaintenanceEvaluationService evaluationService)
        {
            _context = context;
            _evaluationService = evaluationService;
        }

        public async Task<IEnumerable<VehicleMaintenanceStatusDto>?> GetVehicleMaintenanceStatusAsync(int vehicleId, CancellationToken ct)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId, ct);
            if (vehicle == null) return null;

            // pobranie wszystkich typów przeglądów
            var maintenanceTypes = await _context.MaintenanceTypes.ToListAsync(ct);
            var results = new List<VehicleMaintenanceStatusDto>();

            // przechodzenie w petli po wszystkich typach
            foreach (var type in maintenanceTypes)
            {
                var lastEvent = await _context.MaintenanceEvents
                    .Where(m => m.VehicleId == vehicleId && m.MaintenanceTypeId == type.Id)
                    .OrderByDescending(m => m.Date)
                    .FirstOrDefaultAsync(ct);

                DateTime baselineDate = lastEvent?.Date ?? vehicle.CreatedAt;

                // paliwo dolane od poprzedniego serwisu tego typu
                double fuelConsumed = await _context.FuelingEvents
                    .Where(f => f.VehicleId == vehicleId && f.Date > baselineDate)
                    .SumAsync(f => f.LitersAdded, ct);

                // Zderzenie danych ze strategiami
                var status = _evaluationService.EvaluateVehicleMaintenance(
                    vehicle,
                    type,
                    lastEvent,
                    fuelConsumed
                );

                var dto = new VehicleMaintenanceStatusDto(
                    MaintenanceTypeId: type.Id,            
                    MaintenanceTypeName: type.Name,
                    StatusLevel: status.Level.ToString(), // Konwersja ENUM do String
                    Message: status.Message,
                    KilometersRemaining: status.KilometersRemaining,
                    DaysRemaining: status.DaysRemaining,
                    PredictedMaintenanceDate: status.PredictedMaintenanceDate
                );

                results.Add(dto);
            }

            return results;
        }
    }
}
