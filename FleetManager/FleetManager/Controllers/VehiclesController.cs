using FleetManager.Data;
using FleetManager.DTOs; 
using FleetManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FleetManager.Common.Results;

namespace FleetManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly Services.IFuelingService _fuelingService;
        private readonly Services.IVehicleService _vehicleService;

        // Konstruktor: 
        public VehiclesController(AppDbContext context, Services.IFuelingService fuelingService, Services.IVehicleService vehicleService)
        {
            _context = context;
            _fuelingService = fuelingService;
            _vehicleService = vehicleService;
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

        // pobieranie statystyk dot paliwa:
        [HttpGet("{id}/FuelStatistics")]
        public async Task<ActionResult<FuelStatisticsDto>> GetFuelStatistics(int id, CancellationToken ct)
        {
            var stats = await _fuelingService.GetFuelStatisticsAsync(id, ct);

            if (stats == null)
            {
                return NotFound(new { message = $"Pojazd o ID {id} nie istnieje w systemie." });
            }

            return Ok(stats);
        }

        // Dodawanie nowego pojazdu
        [HttpPost]
        public async Task<ActionResult<VehicleCreatedResponseDto>> PostVehicle(VehicleCreateDto dto, CancellationToken ct)
        {
            var result = await _vehicleService.CreateVehicleAsync(dto, ct);

            if (!result.IsSuccess)
            {
                return result.ErrorType switch
                {
                    ResultErrorType.Validation => BadRequest(new { message = result.Error }),
                    _ => StatusCode(500, new { message = "Krytyczny błąd serwera." })
                };
            }

            var responseDto = new VehicleCreatedResponseDto { Id = result.Value!.Id };
            return CreatedAtAction(nameof(GetVehicle), new { id = result.Value.Id }, responseDto);
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
            var result = await _vehicleService.UpdateVehicleAsync(id, dto, ct);

            if (!result.IsSuccess)
            {
                return result.ErrorType switch
                {
                    ResultErrorType.NotFound => NotFound(new { message = result.Error }),
                    ResultErrorType.Validation => BadRequest(new { message = result.Error }),
                    // Wydobycie ładunku błędu i wysłanie go w odpowiedzi JSON
                    ResultErrorType.Conflict => Conflict(result.Value),
                    _ => StatusCode(500, new { message = "Krytyczny błąd serwera." })
                };
            }

            return NoContent();
        }

        //oddzielny endpoint do modyfikacji stanu licznika
        [HttpPatch("{id}/calibrate-odometer")]
        public async Task<IActionResult> CalibrateOdometer(int id, VehicleOdometerCalibrationDto dto, CancellationToken ct)
        {
            var result = await _vehicleService.CalibrateOdometerAsync(id, dto, ct);
            return result.IsSuccess ? NoContent() : HandleErrorResult(result);
        }

        // start symulatora jazdy
        [HttpPost("{id}/start-trip")]
        public async Task<IActionResult> StartTrip(int id, CancellationToken ct)
        {
            var result = await _vehicleService.StartTripAsync(id, ct);
            return result.IsSuccess ? NoContent() : HandleErrorResult(result);
        }

        // zatrzymanie symulatora jazdy
        [HttpPost("{id}/end-trip")]
        public async Task<IActionResult> EndTrip(int id, CancellationToken ct)
        {
            var result = await _vehicleService.EndTripAsync(id, ct);
            return result.IsSuccess ? NoContent() : HandleErrorResult(result);
        }


        //Usuwanie pojazdu po id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id, CancellationToken ct)
        {
            var result = await _vehicleService.DeleteVehicleAsync(id, ct);
            return result.IsSuccess ? NoContent() : HandleErrorResult(result);
        }
        
        //meoda pomocnicza:
        private IActionResult HandleErrorResult(Result result)
        {
            return result.ErrorType switch
            {
                ResultErrorType.NotFound => NotFound(new { message = result.Error }),
                ResultErrorType.Validation => BadRequest(new { message = result.Error }),
                ResultErrorType.Conflict => Conflict(new { message = result.Error }),
                _ => StatusCode(500, new { message = "Krytyczny błąd serwera." })
            };
        }
    }
}
