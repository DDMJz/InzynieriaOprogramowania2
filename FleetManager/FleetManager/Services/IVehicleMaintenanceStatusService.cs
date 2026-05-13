using FleetManager.Common.Results;
using FleetManager.DTOs;

namespace FleetManager.Services
{
    public interface IVehicleMaintenanceStatusService
    {
        Task<Result<IEnumerable<VehicleMaintenanceStatusDto>>> GetVehicleMaintenanceStatusAsync(int vehicleId, CancellationToken ct);
    }
}
