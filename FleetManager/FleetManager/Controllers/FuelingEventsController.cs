using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FleetManager.Data;
using FleetManager.DTOs;
using FleetManager.Common.Results;

namespace FleetManager.Controllers
{
// Kontroler REST: zarządzanie zdarzeniami tankowania
[Route("api/[controller]")]
[ApiController]
public class FuelingEventsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly Services.IFuelingService _fuelingService;

        public FuelingEventsController(AppDbContext context, Services.IFuelingService fuelingService)
        {
            _context = context;
            _fuelingService = fuelingService;
        }

        // GET: api/FuelingEvents/Vehicle/{vehicleId}
        // Pobieranie całej historii tankowań dla konkretnego vehicle
        [HttpGet("Vehicle/{vehicleId}")]
        public async Task<ActionResult<IEnumerable<FuelingEventReadDto>>> GetVehicleHistory(int vehicleId, CancellationToken ct)
        {
            var history = await _context.FuelingEvents
                .Where(f => f.VehicleId == vehicleId)
                .OrderByDescending(f => f.Date)
                .Select(f => new FuelingEventReadDto
                {
                    Id = f.Id,
                    VehicleId = f.VehicleId,
                    OdometerReading = f.OdometerReading,
                    LitersAdded = f.LitersAdded,
                    Cost = f.Cost,
                    Date = f.Date,
                    RowVersion = f.RowVersion
                })
                .ToListAsync(ct);

            return Ok(history);
        }

        // GET: api/FuelingEvents/{id}
        // Pobieranie po id 
        [HttpGet("{id}")]
        public async Task<ActionResult<FuelingEventReadDto>> GetFuelingEvent(int id, CancellationToken ct)
        {
            var dto = await _context.FuelingEvents
                .Where(f => f.Id == id)
                .Select(f => new FuelingEventReadDto
                {
                    Id = f.Id,
                    VehicleId = f.VehicleId, 
                    OdometerReading = f.OdometerReading,
                    LitersAdded = f.LitersAdded,
                    Cost = f.Cost,
                    Date = f.Date,
                    RowVersion = f.RowVersion
                })
                .FirstOrDefaultAsync(ct);

            if (dto == null)
            {
                return NotFound();
            }
            // zwraca DTO ze zdarzeniem tankowania
            return Ok(dto);
        }

        // POST: api/FuelingEvents
        //dodanie nowego tankowania
        // Deleguje logikę tworzenia tankowania do serwisu biznesowego
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]//to jest niestandardowe(niegodne z domyslna konwencja)
        public async Task<IActionResult> PostFuelingEvent(FuelingEventCreateDto dto, CancellationToken ct)
        {
            var result = await _fuelingService.CreateFuelingAsync(dto, ct);
            
            if (!result.IsSuccess)
            {
                return result.ErrorType switch
                {
                    ResultErrorType.NotFound => NotFound(new { message = result.Error }),
                    ResultErrorType.Validation => BadRequest(new { message = result.Error }),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = "Wystąpił nieoczekiwany błąd wewnętrzny serwera." })
                };
            }

            var responseDto = new FuelingEventCreatedResponseDto { Id = result.Value!.Id };
            return CreatedAtAction(nameof(GetFuelingEvent), new { id = result.Value.Id }, responseDto);
        }

        // DELETE: api/FuelingEvents/{id}
        //usuwanie tankowania
        // Deleguje usuwanie tankowania do serwisu
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFuelingEvent(int id, CancellationToken ct)
        {
            var result = await _fuelingService.DeleteFuelingAsync(id, ct);
            
            if (!result.IsSuccess)
            {
                return result.ErrorType switch
                {
                    ResultErrorType.NotFound => NotFound(new { message = result.Error }),
                    ResultErrorType.Validation => BadRequest(new { message = result.Error }),
                    ResultErrorType.Conflict => Conflict(new { message = result.Error }),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = "Wystąpił nieoczekiwany błąd wewnętrzny serwera." })
                };
            }

            return NoContent();
        }
    }
}
