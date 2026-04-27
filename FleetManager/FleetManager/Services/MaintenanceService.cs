using FleetManager.Data;
using FleetManager.DTOs;
using FleetManager.Models;
using Microsoft.EntityFrameworkCore;
using FleetManager.Common.Results;

namespace FleetManager.Services
{
    
    public class MaintenanceService : IMaintenanceService
    {
        private readonly AppDbContext _context;

        public MaintenanceService(AppDbContext context)
        {
            _context = context;
        }

        // Tworzy zdarzenie serwisowe i aktualizuje przebieg pojazdu
        public async Task<Result<MaintenanceEvent>> CreateMaintenanceAsync(MaintenanceEventCreateDto dto, CancellationToken ct)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == dto.VehicleId, ct);
            if (vehicle == null)
            {
                return Result<MaintenanceEvent>.NotFound($"Pojazd o ID {dto.VehicleId} nie istnieje.");
            }

            var type = await _context.MaintenanceTypes.FirstOrDefaultAsync(t => t.Id == dto.MaintenanceTypeId, ct);
            if (type == null)
            {
                return Result<MaintenanceEvent>.NotFound($"Typ naprawy o ID {dto.MaintenanceTypeId} nie istnieje.");
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

            if (dto.OdometerReading > vehicle.OdometerReading)
            {
                vehicle.OdometerReading = dto.OdometerReading;
            }

            _context.MaintenanceEvents.Add(maintenanceEvent);

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                return Result<MaintenanceEvent>.Validation($"Błąd zapisu do bazy danych: {ex.Message}");
            }

            return Result<MaintenanceEvent>.Success(maintenanceEvent);
        }

        // Usuwa zdarzenie serwisowe (przebieg nie jest w ogole sprawdzany)
        public async Task<Result> DeleteMaintenanceAsync(int id, CancellationToken ct)
        {
            var mEvent = await _context.MaintenanceEvents
                .FirstOrDefaultAsync(m => m.Id == id, ct);

            if (mEvent == null)
            {
                return Result.NotFound($"Zdarzenie serwisowe o ID {id} nie istnieje.");
            }

            _context.MaintenanceEvents.Remove(mEvent);

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result.Validation("Zdarzenie zostało zmodyfikowane lub usunięte przez inny proces.");
            }

            return Result.Success();
        }
    }
}
