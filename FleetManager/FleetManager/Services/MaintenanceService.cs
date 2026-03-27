using FleetManager.Data;
using FleetManager.DTOs;
using FleetManager.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetManager.Services
{
    // Implementacja serwisu eksploatacji: reguły tworzenia/usuwania zdarzeń serwisowych
    public class MaintenanceService : IMaintenanceService
    {
        private readonly AppDbContext _context;

        public MaintenanceService(AppDbContext context)
        {
            _context = context;
        }

        // Tworzy zdarzenie serwisowe i aktualizuje przebieg pojazdu
        public async Task<(bool Success, string? Error, MaintenanceEvent? Event)> CreateMaintenanceAsync(MaintenanceEventCreateDto dto, CancellationToken ct)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == dto.VehicleId, ct);
            if (vehicle == null)
            {
                return (false, $"NotFound:Pojazd o ID {dto.VehicleId} nie istnieje.", null);
            }

            var type = await _context.MaintenanceTypes.FirstOrDefaultAsync(t => t.Id == dto.MaintenanceTypeId, ct);
            if (type == null)
            {
                return (false, $"Typ naprawy o ID {dto.MaintenanceTypeId} nie istnieje w bazie.", null);
            }

            // walidacja przebiegu
            if (dto.OdometerReading < vehicle.OdometerReading)
            {
                return (false, $"Podany przebieg ({dto.OdometerReading}) jest mniejszy niz aktualny przebieg pojazdu ({vehicle.OdometerReading}).", null);
            }

            var maintenanceEvent = new MaintenanceEvent
            {
                VehicleId = dto.VehicleId,
                MaintenanceTypeId = dto.MaintenanceTypeId,
                OdometerReading = dto.OdometerReading,
                TotalCost = dto.TotalCost,
                Description = dto.Description,
                Date = dto.Date
            };

            vehicle.OdometerReading = dto.OdometerReading;

            _context.MaintenanceEvents.Add(maintenanceEvent);

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                return (false, ex.Message, null);
            }

            return (true, null, maintenanceEvent);
        }

        // Usuwa zdarzenie serwisowe i cofa przebieg pojazdu do ostatniego znanego zdarzenia
        public async Task<(bool Success, string? Error)> DeleteMaintenanceAsync(int id, CancellationToken ct)
        {
            var mEvent = await _context.MaintenanceEvents.Include(m => m.Vehicle).FirstOrDefaultAsync(m => m.Id == id, ct);
            if (mEvent == null)
            {
                return (false, $"NotFound:Zdarzenie serwisowe o ID {id} nie istnieje.");
            }

            var vehicle = mEvent.Vehicle;
            if (vehicle != null)
            {
                // cofnięcie przebiegu do ostatniego zdarzenia (tankowanie lub inne serwisowe)
                var lastFuel = await _context.FuelingEvents
                    .Where(f => f.VehicleId == vehicle.Id)
                    .OrderByDescending(f => f.Date)
                    .ThenByDescending(f => f.Id)
                    .FirstOrDefaultAsync(ct);

                var lastMaintenance = await _context.MaintenanceEvents
                    .Where(m => m.VehicleId == vehicle.Id && m.Id != id)
                    .OrderByDescending(m => m.Date)
                    .ThenByDescending(m => m.Id)
                    .FirstOrDefaultAsync(ct);

                int newOdometer = 0;
                if (lastFuel != null || lastMaintenance != null)
                {
                    newOdometer = Math.Max(lastFuel?.OdometerReading ?? 0, lastMaintenance?.OdometerReading ?? 0);
                }

                vehicle.OdometerReading = newOdometer;
            }

            _context.MaintenanceEvents.Remove(mEvent);
            await _context.SaveChangesAsync(ct);

            return (true, null);
        }
    }
}
