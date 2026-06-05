using FleetManager.Common.Results;
using FleetManager.Data;
using FleetManager.DTOs; 
using FleetManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ApiBaseController
    {
        private readonly AppDbContext _context;
        private readonly Services.IFuelingService _fuelingService;
        private readonly Services.IVehicleService _vehicleService;
        private readonly IVehicleMaintenanceStatusService _maintenanceStatusService;

        // Konstruktor: 
        public VehiclesController(
            AppDbContext context, 
            Services.IFuelingService fuelingService, 
            Services.IVehicleService vehicleService, 
            IVehicleMaintenanceStatusService maintenanceStatusService)
        {
            _context = context;
            _fuelingService = fuelingService;
            _vehicleService = vehicleService;
            _maintenanceStatusService = maintenanceStatusService;
        }

        // Pobieranie wszystkich pojazdów
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleReadDto>>> GetVehicles(CancellationToken ct)
        {
            var vehicles = await _context.Vehicles
               .Select(v => new VehicleReadDto
               {
                   Id = v.Id,
                   Vin = v.Vin,
                   LicensePlate = v.LicensePlate,
                   Brand = v.Brand,
                   Model = v.Model,
                   Year = v.Year,
                   OdometerReading = v.OdometerReading,
                   FuelTankCapacity = v.FuelTankCapacity, 
                   FuelConsumption = v.FuelConsumption,
                   Status = v.Status.ToString(),
                   RowVersion = v.RowVersion //potrzebne dla put
               })
               .ToListAsync(ct);

            return Ok(vehicles);
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
                    FuelTankCapacity = v.FuelTankCapacity, 
                    FuelConsumption = v.FuelConsumption,
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
            var result = await _fuelingService.GetFuelStatisticsAsync(id, ct);

            return result.IsSuccess ? Ok(result.Value) : HandleErrorResult(result);
        }

        // pobieranie statusu serwisowego
        [HttpGet("{id}/maintenance-status")]
        public async Task<ActionResult<IEnumerable<VehicleMaintenanceStatusDto>>> GetMaintenanceStatus(int id, CancellationToken ct)
        {
            var result = await _maintenanceStatusService.GetVehicleMaintenanceStatusAsync(id, ct);

            return result.IsSuccess ? Ok(result.Value) : HandleErrorResult(result);
        }

        // Dodawanie nowego pojazdu
        [HttpPost]
        public async Task<ActionResult<VehicleCreatedResponseDto>> PostVehicle(VehicleCreateDto dto, CancellationToken ct)
        {
            var result = await _vehicleService.CreateVehicleAsync(dto, ct);

            if (!result.IsSuccess)
            {
                return HandleErrorResult(result);
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
                if (result.ErrorType == ResultErrorType.Conflict)
                {
                    return Conflict(result.Value);// Wydobycie ładunku błędu i wysłanie go w odpowiedzi JSON
                }
                return HandleErrorResult(result);
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
    }
}
