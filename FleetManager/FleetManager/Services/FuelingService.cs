using FleetManager.Data;
using FleetManager.DTOs;
using FleetManager.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetManager.Services
{
    // Implementacja serwisu tankowania: zawiera reguły biznesowe dotyczące tworzenia i usuwania tankowań
    //po wprowadzeniu obslugi TelemetryLog nalezy zmienic logike uaktualniania stanu licznika i poziomu paliwa
    public class FuelingService : IFuelingService
    {
        private readonly AppDbContext _context;

        public FuelingService(AppDbContext context)
        {
            _context = context;
        }

        // Tworzy zdarzenie tankowania, aktualizuje przebieg i poziom paliwa pojazdu
        public async Task<(bool Success, string? Error, FuelingEvent? Event)> CreateFuelingAsync(FuelingEventCreateDto dto, CancellationToken ct)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == dto.VehicleId, ct);
            if (vehicle == null)
            {
                return (false, $"NotFound:Pojazd o ID {dto.VehicleId} nie istnieje.", null);
            }

            // walidacja przebiegu
            if (dto.OdometerReading < vehicle.OdometerReading)
            {
                return (false, $"Podany przebieg ({dto.OdometerReading}) jest mniejszy niz aktualny przebieg pojazdu ({vehicle.OdometerReading}).", null);
            }

            // walidacja liczby litrów
            if (dto.LitersAdded <= 0)
            {
                return (false, "Ilość dolewanych paliwa musi być większa niż 0.", null);
            }

            var fuelingEvent = new FuelingEvent
            {
                VehicleId = dto.VehicleId,
                OdometerReading = dto.OdometerReading,
                LitersAdded = dto.LitersAdded,
                TotalCost = dto.TotalCost,
                Date = dto.Date
            };

            // synchronizacja stanu pojazdu
            vehicle.OdometerReading = dto.OdometerReading;
            vehicle.CurrentFuelLevel += dto.LitersAdded;

            _context.FuelingEvents.Add(fuelingEvent);

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                return (false, ex.Message, null);
            }

            return (true, null, fuelingEvent);
        }

        // Usuwa zdarzenie tankowania i cofa jego efekty na pojeździe
        public async Task<(bool Success, string? Error)> DeleteFuelingAsync(int id, CancellationToken ct)
        {
            var fuelingEvent = await _context.FuelingEvents.Include(f => f.Vehicle).FirstOrDefaultAsync(f => f.Id == id, ct);
            if (fuelingEvent == null)
            {
                return (false, $"NotFound:Zdarzenie tankowania o ID {id} nie istnieje.");
            }

            // cofanie wpływu na poziom paliwa
            var vehicle = fuelingEvent.Vehicle;
            if (vehicle != null)
            {
                vehicle.CurrentFuelLevel -= fuelingEvent.LitersAdded;
                if (vehicle.CurrentFuelLevel < 0) vehicle.CurrentFuelLevel = 0;

                // odtworzenie przebiegu na podstawie ostatnich zdarzeń (tankowanie/serwis)
                var lastEvent = await _context.FuelingEvents
                    .Where(f => f.VehicleId == vehicle.Id && f.Id != id)
                    .OrderByDescending(f => f.Date)
                    .ThenByDescending(f => f.Id)
                    .FirstOrDefaultAsync(ct);

                var lastMaintenance = await _context.MaintenanceEvents
                    .Where(m => m.VehicleId == vehicle.Id)
                    .OrderByDescending(m => m.Date)
                    .ThenByDescending(m => m.Id)
                    .FirstOrDefaultAsync(ct);

                int newOdometer = 0;
                if (lastEvent != null || lastMaintenance != null)
                {
                    newOdometer = Math.Max(lastEvent?.OdometerReading ?? 0, lastMaintenance?.OdometerReading ?? 0);
                }

                vehicle.OdometerReading = newOdometer;
            }

            _context.FuelingEvents.Remove(fuelingEvent);

            await _context.SaveChangesAsync(ct);

            return (true, null);
        }
    }
}
