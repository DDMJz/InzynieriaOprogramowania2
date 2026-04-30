using FleetManager.DTOs;

namespace FleetManager.Services
{
    public interface IVehicleMaintenanceStatusService
    {
        Task<IEnumerable<VehicleMaintenanceStatusDto>?> GetVehicleMaintenanceStatusAsync(int vehicleId, CancellationToken ct);
    }
}
