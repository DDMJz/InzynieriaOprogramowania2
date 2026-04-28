using FleetManager.Common.Results;
using FleetManager.Data;
using FleetManager.DTOs;
using FleetManager.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetManager.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VehicleService> _logger; 

        public VehicleService(AppDbContext context, ILogger<VehicleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<Vehicle>> CreateVehicleAsync(VehicleCreateDto dto, CancellationToken ct)
        {
            bool vinExists = await _context.Vehicles.AnyAsync(v => v.Vin == dto.Vin, ct);
            if (vinExists)
            {
                return Result<Vehicle>.Validation($"Pojazd z numerem VIN {dto.Vin} już figuruje w systemie.");
            }

            var vehicle = new Vehicle
            {
                Vin = dto.Vin,
                LicensePlate = dto.LicensePlate,
                Brand = dto.Brand,
                Model = dto.Model,
                OdometerReading = dto.OdometerReading,
                Status = VehicleStatus.Idle
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync(ct);

            return Result<Vehicle>.Success(vehicle);
        }

        public async Task<Result<VehicleUpdateDto>> UpdateVehicleAsync(int id, VehicleUpdateDto dto, CancellationToken ct)
        {
            var vehicleInDb = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id, ct);
            if (vehicleInDb == null)
            {
                return Result<VehicleUpdateDto>.NotFound($"Pojazd o ID {id} nie istnieje.");
            }

            _context.Entry(vehicleInDb).OriginalValues["RowVersion"] = dto.RowVersion;//nadpisywanie RowVersion z bazy tym z zapytania HTTP

            vehicleInDb.LicensePlate = dto.LicensePlate;
            vehicleInDb.Brand = dto.Brand;
            vehicleInDb.Model = dto.Model;
            vehicleInDb.Year = dto.Year;

            try
            {
                await _context.SaveChangesAsync(ct);
                return Result<VehicleUpdateDto>.Success(dto); 
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var databaseValues = await entry.GetDatabaseValuesAsync(ct);//pobieranie z bazy aktualnego stanu obiektu ktory spowodowal konflikt

                if (databaseValues == null)
                {
                    return Result<VehicleUpdateDto>.NotFound("Pojazd został usunięty przez innego użytkownika.");
                }

                await entry.ReloadAsync(ct);
                vehicleInDb = (Vehicle)entry.Entity;

                var updatedDto = new VehicleUpdateDto
                {
                    LicensePlate = vehicleInDb.LicensePlate,
                    Brand = vehicleInDb.Brand,
                    Model = vehicleInDb.Model,
                    Year = vehicleInDb.Year,
                    RowVersion = vehicleInDb.RowVersion
                };

                // aktualny stan obiektu w bazie danych jako value
                return Result<VehicleUpdateDto>.Conflict(updatedDto, "Konflikt współbieżności.");
            }
        }

        public async Task<Result> CalibrateOdometerAsync(int id, VehicleOdometerCalibrationDto dto, CancellationToken ct)
        {
            var vehicle = await _context.Vehicles.FindAsync(new object[] { id }, ct);
            if (vehicle == null)
            {
                return Result.NotFound($"Pojazd o ID {id} nie istnieje.");
            }

            _logger.LogWarning(
                "AUDYT BEZPIECZEŃSTWA: Zmieniono stan licznika pojazdu {VehicleId}. Poprzedni przebieg: {OldOdometer}, Nowy przebieg: {NewOdometer}. Uzasadnienie: {Justification}",
                id, vehicle.OdometerReading, dto.NewOdometerReading, dto.Justification);

            vehicle.OdometerReading = dto.NewOdometerReading;
            await _context.SaveChangesAsync(ct);

            return Result.Success();
        }

        public async Task<Result> StartTripAsync(int id, CancellationToken ct)
        {
            var vehicle = await _context.Vehicles.FindAsync(new object[] { id }, ct);
            if (vehicle == null) return Result.NotFound($"Pojazd o ID {id} nie istnieje.");

            if (vehicle.Status != VehicleStatus.Idle)
            {
                return Result.Validation("Pojazd musi być w stanie Idle, aby rozpocząć jazdę.");
            }

            vehicle.Status = VehicleStatus.InTransit;
            await _context.SaveChangesAsync(ct);
            return Result.Success();
        }

        public async Task<Result> EndTripAsync(int id, CancellationToken ct)
        {
            var vehicle = await _context.Vehicles.FindAsync(new object[] { id }, ct);
            if (vehicle == null) return Result.NotFound($"Pojazd o ID {id} nie istnieje.");

            if (vehicle.Status != VehicleStatus.InTransit)
            {
                return Result.Validation("Tylko pojazd będący w stanie InTransit może zakończyć jazdę.");
            }

            vehicle.Status = VehicleStatus.Idle;
            await _context.SaveChangesAsync(ct);
            return Result.Success();
        }

        public async Task<Result> DeleteVehicleAsync(int id, CancellationToken ct)
        {
            var vehicle = await _context.Vehicles.FindAsync(new object[] { id }, ct);
            if (vehicle == null)
            {
                return Result.NotFound($"Pojazd o ID {id} nie istnieje.");
            }
            _context.Vehicles.Remove(vehicle);
            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result.Conflict("Pojazd został już usunięty lub zmodyfikowany przez innego użytkownika.");
            }
            return Result.Success();
        }
    }
}
