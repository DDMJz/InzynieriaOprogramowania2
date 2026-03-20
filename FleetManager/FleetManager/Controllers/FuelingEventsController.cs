using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FleetManager.Data;
using FleetManager.Models;
using FleetManager.DTOs;

namespace FleetManager.Controllers
{
// Kontroler REST: zarządzanie zdarzeniami tankowania
[Route("api/[controller]")]
[ApiController]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
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
                    TotalCost = f.TotalCost,
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
                    TotalCost = f.TotalCost,
                    Date = f.Date
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
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(FuelingEventCreatedResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // Deleguje logikę tworzenia tankowania do serwisu biznesowego
        public async Task<IActionResult> PostFuelingEvent(FuelingEventCreateDto dto, CancellationToken ct)
        {
            var result = await _fuelingService.CreateFuelingAsync(dto, ct);
            if (!result.Success)
            {
                if (result.Error != null && result.Error.StartsWith("NotFound:"))
                {
                    return NotFound(new { message = result.Error.Substring("NotFound:".Length) });
                }
                return BadRequest(new { message = result.Error });
            }

            var responseDto = new FuelingEventCreatedResponseDto { Id = result.Event!.Id };
            return CreatedAtAction(nameof(GetFuelingEvent), new { id = result.Event.Id }, responseDto);
        }

        // DELETE: api/FuelingEvents/{id}
        //usuwanie tankowania
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // Deleguje usuwanie tankowania (serwis odwraca efekty zdarzenia)
        public async Task<IActionResult> DeleteFuelingEvent(int id, CancellationToken ct)
        {
            var result = await _fuelingService.DeleteFuelingAsync(id, ct);
            if (!result.Success)
            {
                if (result.Error != null && result.Error.StartsWith("NotFound:"))
                {
                    return NotFound(new { message = result.Error.Substring("NotFound:".Length) });
                }
                return BadRequest(new { message = result.Error });
            }

            return NoContent();
        }



    }
}
