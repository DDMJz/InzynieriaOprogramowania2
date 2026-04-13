using FleetManager.DTOs;
using FleetManager.Models;

namespace FleetManager.Services
{
    // Serwis biznesowy: logika związana z tankowaniami
    // (walidacje, aktualizacja stanu pojazdu, cofanie efektów zdarzenia)
    public interface IFuelingService
    {
        // Tworzy nowe zdarzenie tankowania i aktualizuje pojazd
        Task<(bool Success, string? Error, FuelingEvent? Event)> CreateFuelingAsync(FuelingEventCreateDto dto, CancellationToken ct);

        // Usuwa zdarzenie tankowania i cofa jego efekty (poziom paliwa, przebieg)
        Task<(bool Success, string? Error)> DeleteFuelingAsync(int id, CancellationToken ct);
        
        Task<FuelStatisticsDto?> GetFuelStatisticsAsync(int vehicleId, CancellationToken ct);
    }
}
