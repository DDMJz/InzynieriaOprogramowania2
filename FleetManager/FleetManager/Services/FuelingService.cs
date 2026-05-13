using FleetManager.Data;
using FleetManager.DTOs;
using FleetManager.Models;
using Microsoft.EntityFrameworkCore;
using FleetManager.Common.Results;

namespace FleetManager.Services
{
    // Implementacja serwisu tankowania: zawiera reguły biznesowe dotyczące tworzenia i usuwania tankowań
    public class FuelingService : IFuelingService
    {
        private readonly AppDbContext _context;

        public FuelingService(AppDbContext context)
        {
            _context = context;
        }

        // Tworzy zdarzenie tankowania, aktualizuje przebieg i poziom paliwa pojazdu
        public async Task<Result<FuelingEvent>> CreateFuelingAsync(FuelingEventCreateDto dto, CancellationToken ct)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == dto.VehicleId, ct);
            if (vehicle == null)
            {
                return Result<FuelingEvent>.NotFound($"Pojazd o ID {dto.VehicleId} nie istnieje.");
            }

            // walidacja liczby litrów
            if (dto.LitersAdded <= 0)
            {
                return Result<FuelingEvent>.Validation("Ilość dolewanych paliwa musi być większa niż 0.");
            }

            var fuelingEvent = new FuelingEvent
            {
                VehicleId = dto.VehicleId,
                OdometerReading = dto.OdometerReading,
                LitersAdded = dto.LitersAdded,
                Cost = dto.Cost,
                Date = dto.Date,
            };

            // synchronizacja stanu pojazdu
            if (dto.OdometerReading > vehicle.OdometerReading)
            {
                vehicle.OdometerReading = dto.OdometerReading;
            }

            _context.FuelingEvents.Add(fuelingEvent);

            await _context.SaveChangesAsync(ct);

            return Result<FuelingEvent>.Success(fuelingEvent);
        }

        // Usuwa zdarzenie tankowania 
        public async Task<Result> DeleteFuelingAsync(int id, CancellationToken ct)
        {
            var fuelingEvent = await _context.FuelingEvents.FirstOrDefaultAsync(f => f.Id == id, ct);
            
            if (fuelingEvent == null)
            {
                return Result.NotFound($"Zdarzenie tankowania o ID {id} nie istnieje.");
            }

            _context.FuelingEvents.Remove(fuelingEvent);
            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result.Conflict("Zdarzenie tankowania zostało zmodyfikowane lub usunięte przez inny proces.");
            }

            return Result.Success();
        }

        public async Task<Result<FuelStatisticsDto>> GetFuelStatisticsAsync(int vehicleId, CancellationToken ct)
        {
            // walidacja istnienia samochodu
            var vehicleExists = await _context.Vehicles.AnyAsync(v => v.Id == vehicleId, ct);
            if (!vehicleExists)
            {
                return Result<FuelStatisticsDto>.NotFound($"Pojazd o ID {vehicleId} nie istnieje w systemie.");
            }

            // pobranie tankowan
            var fuelingEvents = await _context.FuelingEvents
                .Where(f => f.VehicleId == vehicleId)
                .OrderBy(f => f.OdometerReading)
                .ToListAsync(ct);

            var stats = new FuelStatisticsDto { VehicleId = vehicleId };

            // jezeli mniej niż 2 tankowania, oddawane puste statystyki (brak dystansu)
            if (fuelingEvents.Count < 2)
            {
                return Result<FuelStatisticsDto>.Success(stats);
            }
            
            // obliczenia (zakladamy ze kazde tamkowanie jest do pelna)
            var firstReading = fuelingEvents.First().OdometerReading;
            var lastReading = fuelingEvents.Last().OdometerReading;

            stats.TotalDistanceKm = lastReading - firstReading;

            // pierwsze tankowanie pomijane przy sumowaniu paliwa
            stats.TotalFuelLiters = Math.Round(fuelingEvents.Skip(1).Sum(f => f.LitersAdded), 2);
            stats.TotalCost = Math.Round(fuelingEvents.Skip(1).Sum(f => f.Cost), 2);

            // zabezpieczenie przed dzieleniem przez zero 
            if (stats.TotalDistanceKm > 0)
            {
                stats.AverageConsumption = Math.Round((stats.TotalFuelLiters / stats.TotalDistanceKm) * 100, 2);
            }

            return Result<FuelStatisticsDto>.Success(stats);
        }
    }
}
