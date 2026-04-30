using FleetManager.DTOs;
using FleetManager.Models;
using FleetManager.Common.Results;

namespace FleetManager.Services
{
    // Serwis biznesowy: logika związana z tankowaniami
    // (walidacje, aktualizacja stanu pojazdu, cofanie efektów zdarzenia)
    public interface IFuelingService
    {
        // Tworzy nowe zdarzenie tankowania i aktualizuje pojazd
        Task<Result<FuelingEvent>> CreateFuelingAsync(FuelingEventCreateDto dto, CancellationToken ct);

        // Usuwa zdarzenie tankowania 
        Task<Result> DeleteFuelingAsync(int id, CancellationToken ct);
        
        Task<Result<FuelStatisticsDto>> GetFuelStatisticsAsync(int vehicleId, CancellationToken ct);
    }
}
