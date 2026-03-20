using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FleetManager.Data;
using FleetManager.Models;
using FleetManager.DTOs; //katalog z dto -data transfer object-sluzy do filtrowania zapytan

namespace FleetManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class VehiclesController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Konstruktor: 
        public VehiclesController(AppDbContext context)
        {
            _context = context;
        }

        // Pobieranie wszystkich pojazdów
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleReadDto>>> GetVehicles(CancellationToken ct)
        {
            return await _context.Vehicles
                .Select(v => new VehicleReadDto
                {
                    Id = v.Id,
                    Vin = v.Vin,
                    LicensePlate = v.LicensePlate,
                    Brand = v.Brand,
                    Model = v.Model,
                    OdometerReading = v.OdometerReading,
                    CurrentFuelLevel = v.CurrentFuelLevel,
                    RowVersion = v.RowVersion //potrzebne dla put 
                })
                .ToListAsync(ct);
        }

        // Pobieranie pojazdu po id (kluczu glownym)
        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleReadDto>> GetVehicle(int id, CancellationToken ct)
        {
            var vehicleDto = await _context.Vehicles
                .Where(v => v.Id == id)
                .Select(v => new VehicleReadDto
                {
                    Id = v.Id,
                    Vin = v.Vin,
                    LicensePlate = v.LicensePlate,
                    Brand = v.Brand,
                    Model = v.Model,
                    Year = v.Year,
                    OdometerReading = v.OdometerReading,
                    CurrentFuelLevel = v.CurrentFuelLevel,
                    RowVersion = v.RowVersion //potrzebne dla put
                })
                .FirstOrDefaultAsync(ct);
                       
            if (vehicleDto == null)
            {
                return NotFound(new { message = $"Nie ma pojazdu o ID {id} w systemie." });
            }

            return Ok(vehicleDto);
        }

        // Dodawanie nowego pojazdu
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(VehicleCreatedResponseDto))]
        public async Task<IActionResult> PostVehicle(VehicleCreateDto dto, CancellationToken ct)
        {
            bool vinExists = await _context.Vehicles.AnyAsync(v => v.Vin == dto.Vin, ct);

            if (vinExists) return BadRequest(new { message = $"Pojazd z numerem VIN {dto.Vin} już figuruje w systemie." });
            
            var vehicle = new Vehicle
            {
                Vin = dto.Vin,
                LicensePlate = dto.LicensePlate,
                Brand = dto.Brand,
                Model = dto.Model, 
                OdometerReading = dto.OdometerReading
            };
            
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync(ct);
            
            var responseDto = new VehicleCreatedResponseDto { Id = vehicle.Id };
            return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, responseDto);
        }

        // Edycja pojazdu
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)] 
        [ProducesResponseType(StatusCodes.Status404NotFound)] 
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(VehicleUpdateDto))]
        public async Task<IActionResult> PutVehicle(int id, VehicleUpdateDto dto, CancellationToken ct)
        {
            
            var vehicleInDb = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id, ct);

            if (vehicleInDb == null)
            {
                return NotFound(new { message = $"Pojazd o ID {id} nie istnieje." });
            }

            _context.Entry(vehicleInDb).OriginalValues["RowVersion"] = dto.RowVersion;

            vehicleInDb.LicensePlate = dto.LicensePlate;
            vehicleInDb.Brand = dto.Brand;
            vehicleInDb.Model = dto.Model;
            vehicleInDb.Year = dto.Year;

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();

                var databaseValues = await entry.GetDatabaseValuesAsync(ct);

                if (databaseValues == null)
                {
                    return NotFound(new { message = "Pojazd został usunięty przez innego użytkownika." });
                }
                await entry.ReloadAsync(ct);
                vehicleInDb = (Vehicle)entry.Entity;

                var updatedDto = new VehicleUpdateDto
                {
                    LicensePlate = vehicleInDb.LicensePlate,
                    Brand = vehicleInDb.Brand,
                    Model = vehicleInDb.Model,
                    Year = vehicleInDb.Year,
                    RowVersion = vehicleInDb.RowVersion // aktualne
                };

                return Conflict(updatedDto);
            }

            return NoContent();
        }

        //Usuwanie pojazdu po id
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteVehicle(int id, CancellationToken ct)
        {
            var vehicle = await _context.Vehicles.FindAsync(new object[] { id }, ct);
            if (vehicle == null)
            {
                return NotFound();
            }

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync(ct);

            return NoContent(); 
        }
    }
}
