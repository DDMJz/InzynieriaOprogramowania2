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
    public class MaintenanceEventsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MaintenanceEventsController(AppDbContext context)
        {
            _context = context;
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

            return Ok(dto);
        }

        // POST: api/MaintenanceEvents
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MaintenanceEventCreatedResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostMaintenanceEvent(MaintenanceEventCreateDto dto, CancellationToken ct)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == dto.VehicleId, ct);
            if (vehicle == null)
            {
                return NotFound(new { message = $"Pojazd o ID {dto.VehicleId} nie istnieje." });
            }

            var typeExists = await _context.MaintenanceTypes.AnyAsync(t => t.Id == dto.MaintenanceTypeId, ct);
            if (!typeExists)
            {
                return BadRequest(new { message = $"Typ naprawy o ID {dto.MaintenanceTypeId} nie istnieje w bazie." });
            }

            if (dto.OdometerReading < vehicle.OdometerReading)
            {
                return BadRequest(new
                {
                    message = "Blad licznika.",
                    details = $"Podany przebieg ({dto.OdometerReading}) jest mniejszy niz aktualny przebieg pojazdu ({vehicle.OdometerReading})."
                });
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

            vehicle.OdometerReading = dto.OdometerReading;

            _context.MaintenanceEvents.Add(maintenanceEvent);

            await _context.SaveChangesAsync(ct);

            
            var responseDto = new MaintenanceEventCreatedResponseDto { Id = maintenanceEvent.Id };

            return CreatedAtAction(nameof(GetMaintenanceEvent), new { id = maintenanceEvent.Id }, responseDto);
        }

        // DELETE: api/MaintenanceEvents/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMaintenanceEvent(int id, CancellationToken ct)
        {
            var mEvent = await _context.MaintenanceEvents.FindAsync(new object[] { id }, ct);
            if (mEvent == null)
            {
                return NotFound();
            }
            _context.MaintenanceEvents.Remove(mEvent);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }
    }

}
