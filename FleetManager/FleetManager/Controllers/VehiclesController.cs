using FleetManager.Data;
using FleetManager.DTOs; //katalog z dto -data transfer object-sluzy do filtrowania zapytan
using FleetManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly Services.IFuelingService _fuelingService;
        private readonly ILogger<VehiclesController> _logger;

        // Konstruktor: 
        public VehiclesController(AppDbContext context, Services.IFuelingService fuelingService, ILogger<VehiclesController> logger)
        {
            _context = context;
            _fuelingService = fuelingService;
            _logger = logger;
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
                    Year = v.Year,
                    OdometerReading = v.OdometerReading,
                    Status = v.Status.ToString(),
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
                    Status = v.Status.ToString(),
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
        public async Task<ActionResult<VehicleCreatedResponseDto>> PostVehicle(VehicleCreateDto dto, CancellationToken ct)
        {
            bool vinExists = await _context.Vehicles.AnyAsync(v => v.Vin == dto.Vin, ct);

            if (vinExists) return BadRequest(new { message = $"Pojazd z numerem VIN {dto.Vin} już figuruje w systemie." });
            
            var vehicle = new Vehicle
            {
                Vin = dto.Vin,
                LicensePlate = dto.LicensePlate,
                Brand = dto.Brand,
                Model = dto.Model, 
                OdometerReading = dto.OdometerReading,
                Status = VehicleStatus.Idle // stan domyslny
            };
            
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync(ct);
            
            var responseDto = new VehicleCreatedResponseDto { Id = vehicle.Id };
            return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, responseDto);
        }

        // Edycja pojazdu
        // w razie konfliktu wspolbieznosci zwraca aktualny stan rekordu w bazie 
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)] 
        [ProducesResponseType(StatusCodes.Status404NotFound)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(VehicleUpdateDto))]
        public async Task<IActionResult> PutVehicle(int id, VehicleUpdateDto dto, CancellationToken ct)
        {
            
            var vehicleInDb = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id, ct);

            if (vehicleInDb == null)
            {
                return NotFound(new { message = $"Pojazd o ID {id} nie istnieje." });
            }
            //dto.RowVersion przysyla klient - ma je z poprzedniego zapytania get
            //tu nadpisywane do obrazu rekordu w pamieci
            _context.Entry(vehicleInDb).OriginalValues["RowVersion"] = dto.RowVersion;

            vehicleInDb.LicensePlate = dto.LicensePlate;
            vehicleInDb.Brand = dto.Brand;
            vehicleInDb.Model = dto.Model;
            vehicleInDb.Year = dto.Year;

            try
            {
                //jezeli RowVersion w miedzyczasie sie zmienilo w bazie (modyfikacja rekordu) tu wyrzucony zostanie wyjatek
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var databaseValues = await entry.GetDatabaseValuesAsync(ct);

                if (databaseValues == null)
                {
                    //jezeli rekord w bazie zostal w miedzyczasie skasowany
                    return NotFound(new { message = "Pojazd został usunięty przez innego użytkownika." });
                }
                //pobranie aktualnej wersji rekordu z bazy
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
                //zwraca aktualny stan rekordu w bazie jezeli w miedzyczasie sie zmienil
                return Conflict(updatedDto);
            }

            return NoContent();
        }

        //oddzielny endpoint do modyfikacji stanu licznika
        [HttpPatch("{id}/calibrate-odometer")]
        public async Task<IActionResult> CalibrateOdometer(int id, VehicleOdometerCalibrationDto dto, CancellationToken ct)
        {
            var vehicle = await _context.Vehicles.FindAsync(new object[] { id }, ct);
            if (vehicle == null) return NotFound();

            _logger.LogWarning(
                "AUDYT BEZPIECZEŃSTWA: Zmieniono stan licznika pojazdu {VehicleId}. Poprzedni przebieg: {OldOdometer}, Nowy przebieg: {NewOdometer}. Uzasadnienie: {Justification}",
                id,
                vehicle.OdometerReading,
                dto.NewOdometerReading,
                dto.Justification
                );

            vehicle.OdometerReading = dto.NewOdometerReading;

            await _context.SaveChangesAsync(ct);
            return NoContent();
        }

        // start symulatora telemetrii
        [HttpPost("{id}/start-trip")]
        public async Task<IActionResult> StartTrip(int id, CancellationToken ct)
        {
            var vehicle = await _context.Vehicles.FindAsync(new object[] { id }, ct);
            if (vehicle == null) return NotFound();

            if (vehicle.Status != VehicleStatus.Idle)
            {
                return BadRequest(new { message = "Pojazd musi byc w stanie Idle, aby rozpoczac jazde." });
            }

            vehicle.Status = VehicleStatus.InTransit;
            await _context.SaveChangesAsync(ct);
            return NoContent();
        }

        // zatrzymanie symulatora telemetrii
        [HttpPost("{id}/end-trip")]
        public async Task<IActionResult> EndTrip(int id, CancellationToken ct)
        {
            var vehicle = await _context.Vehicles.FindAsync(new object[] { id }, ct);
            if (vehicle == null) return NotFound();

            if (vehicle.Status != VehicleStatus.InTransit)
            {
                return BadRequest(new { message = "Tylko pojazd bedacy w stnie InTransit moze zakonczyc jazde." });
            }

            vehicle.Status = VehicleStatus.Idle;
            await _context.SaveChangesAsync(ct);
            return NoContent();
        }


        //Usuwanie pojazdu po id
        [HttpDelete("{id}")]
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

        // pobieranie statystyk dot paliwa:
        [HttpGet("{id}/FuelStatistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FuelStatisticsDto>> GetFuelStatistics(int id, CancellationToken ct)
        {
            var stats = await _fuelingService.GetFuelStatisticsAsync(id, ct);

            if (stats == null)
            {
                return NotFound(new { message = $"Pojazd o ID {id} nie istnieje w systemie." });
            }

            return Ok(stats);
        }
    }
}
