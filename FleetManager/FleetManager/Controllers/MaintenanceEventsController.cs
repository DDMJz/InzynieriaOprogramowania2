using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FleetManager.Data;
using FleetManager.DTOs;
using FleetManager.Common.Results;

namespace FleetManager.Controllers
{
    // Kontroler REST: zarządzanie zdarzeniami serwisowymi (eksploatacja)
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceEventsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly Services.IMaintenanceService _maintenanceService;

        public MaintenanceEventsController(AppDbContext context, Services.IMaintenanceService maintenanceService)
        {
            _context = context;
            _maintenanceService = maintenanceService;
        }

        // GET: api/MaintenanceEvents/Vehicle/{vehicleId}
        [HttpGet("Vehicle/{vehicleId}")]
        public async Task<ActionResult<IEnumerable<MaintenanceEventReadDto>>> GetVehicleHistory(int vehicleId, CancellationToken ct)
        {
            var history = await _context.MaintenanceEvents
                .Where(m => m.VehicleId == vehicleId)
                .OrderByDescending(m => m.Date)
                .Select(m => new MaintenanceEventReadDto
                {
                    Id = m.Id,
                    VehicleId = m.VehicleId,
                    MaintenanceTypeId = m.MaintenanceTypeId,
                    MaintenanceTypeName = m.MaintenanceType.Name, 
                    OdometerReading = m.OdometerReading,
                    TotalCost = m.TotalCost,
                    Description = m.Description,
                    Date = m.Date,
                    RowVersion = m.RowVersion
                })
                .ToListAsync(ct);
            
            return Ok(history);
        }

        // GET: api/MaintenanceEvents/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<MaintenanceEventReadDto>> GetMaintenanceEvent(int id, CancellationToken ct)
        {
            var dto = await _context.MaintenanceEvents
                .Where(m => m.Id == id)
                .Select(m => new MaintenanceEventReadDto
                {
                    Id = m.Id,
                    VehicleId = m.VehicleId,
                    MaintenanceTypeId = m.MaintenanceTypeId,
                    MaintenanceTypeName = m.MaintenanceType.Name,
                    OdometerReading = m.OdometerReading,
                    TotalCost = m.TotalCost,
                    Description = m.Description,
                    Date = m.Date,
                    RowVersion = m.RowVersion
                })
                .FirstOrDefaultAsync(ct);

            if (dto == null)
            {
                return NotFound();
            }
            // zwraca DTO ze zdarzeniem serwisowym
            return Ok(dto);
        }

        // POST: api/MaintenanceEvents
        // Deleguje tworzenie zdarzenia serwisowego do serwisu biznesowego
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]//to jest niestandardowe(niegodne z globalna konwencja w FleetApiConventions.cs )
        public async Task<IActionResult> PostMaintenanceEvent(MaintenanceEventCreateDto dto, CancellationToken ct)
        {
            var result = await _maintenanceService.CreateMaintenanceAsync(dto, ct);
            
            if (!result.IsSuccess)
            {
                return result.ErrorType switch
                {
                    ResultErrorType.NotFound => NotFound(new { message = result.Error }),
                    ResultErrorType.Validation => BadRequest(new { message = result.Error }),
                    _ => StatusCode(500, new { message = "Krytyczny błąd serwera." })
                };
            }

            var responseDto = new MaintenanceEventCreatedResponseDto { Id = result.Value!.Id };

            return CreatedAtAction(nameof(GetMaintenanceEvent), new { id = result.Value.Id }, responseDto);
        }

        // DELETE: api/MaintenanceEvents/{id}
        // Deleguje usuwanie zdarzenia serwisowego 
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMaintenanceEvent(int id, CancellationToken ct)
        {
            var result = await _maintenanceService.DeleteMaintenanceAsync(id, ct);
            if (!result.IsSuccess)
            {
                return result.ErrorType switch
                {
                    ResultErrorType.NotFound => NotFound(new { message = result.Error }),
                    ResultErrorType.Conflict => Conflict(new { message = result.Error }),
                    _ => StatusCode(500, new { message = "Krytyczny błąd serwera." })
                };
            }

            return NoContent();
        }
    }

}
