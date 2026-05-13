using FleetManager.DTOs;
using FleetManager.Models;
using FleetManager.Common.Results;

namespace FleetManager.Services
{
    public interface IVehicleService
    {
        Task<Result<Vehicle>> CreateVehicleAsync(VehicleCreateDto dto, CancellationToken ct);
        Task<Result<VehicleUpdateDto>> UpdateVehicleAsync(int id, VehicleUpdateDto dto, CancellationToken ct);
        Task<Result> CalibrateOdometerAsync(int id, VehicleOdometerCalibrationDto dto, CancellationToken ct);
        Task<Result> StartTripAsync(int id, CancellationToken ct);
        Task<Result> EndTripAsync(int id, CancellationToken ct);
        Task<Result> DeleteVehicleAsync(int id, CancellationToken ct);
    }
}
