using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FleetManager.Data;
using FleetManager.Models;
using FleetManager.DTOs;

namespace FleetManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class FuelingEventsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FuelingEventsController(AppDbContext context)
        {
            _context = context;
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
            
            return Ok(dto);
        }

        // POST: api/FuelingEvents
        //dodanie nowego tankowania
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(FuelingEventCreatedResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostFuelingEvent(FuelingEventCreateDto dto, CancellationToken ct)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == dto.VehicleId, ct);

            if (vehicle == null)
            {
                return NotFound(new { message = $"Pojazd o ID {dto.VehicleId} nie istnieje." });
            }

            if (dto.OdometerReading < vehicle.OdometerReading)
            {
                return BadRequest(new
                {
                    message = "Blad licznika.",
                    details = $"Podany przebieg ({dto.OdometerReading}) jest mniejszy niz aktualny przebieg pojazdu ({vehicle.OdometerReading})."
                });
            }

            var fuelingEvent = new FuelingEvent
            {
                VehicleId = dto.VehicleId,
                OdometerReading = dto.OdometerReading,
                LitersAdded = dto.LitersAdded,
                TotalCost = dto.TotalCost,
                Date = dto.Date
            };

            // Aktualizacja stanu pojazdu (Synchronizacja danych)
            vehicle.OdometerReading = dto.OdometerReading;
            vehicle.CurrentFuelLevel += dto.LitersAdded;

            _context.FuelingEvents.Add(fuelingEvent);

            await _context.SaveChangesAsync(ct);

            var responseDto = new FuelingEventCreatedResponseDto { Id = fuelingEvent.Id };
            return CreatedAtAction(nameof(GetFuelingEvent), new { id = fuelingEvent.Id }, responseDto);
        }

        // DELETE: api/FuelingEvents/{id}
        //usuwanie tankowania
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteFuelingEvent(int id, CancellationToken ct)
        {
            var fuelingEvent = await _context.FuelingEvents.FindAsync(new object[] { id }, ct);

            if (fuelingEvent == null)
            {
                return NotFound();
            }

            _context.FuelingEvents.Remove(fuelingEvent);

            await _context.SaveChangesAsync(ct);

            return NoContent();
        }



    }
}
