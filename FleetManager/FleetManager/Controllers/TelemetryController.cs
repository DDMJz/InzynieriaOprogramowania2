using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FleetManager.Data;
using FleetManager.DTOs;
using FleetManager.Models;
using FleetManager.Services;



namespace FleetManager.Controllers
{
        //W tej chwili nie uzywany 
/*
    [Route("api/[controller]")]
    [ApiController]
    public class TelemetryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITelemetryService _telemetryService;

        public TelemetryController(AppDbContext context, ITelemetryService telemetryService)
        {
            _context = context;
            _telemetryService = telemetryService;
        }

        // POST: api/Telemetry
        // Dodaj log telemetryczny (symulacja z IoT)
        [HttpPost]
        public async Task<IActionResult> PostTelemetry(TelemetryLogCreateDto dto, CancellationToken ct)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == dto.VehicleId, ct);
            if (vehicle == null)
            {
                return NotFound(new { message = $"Pojazd o ID {dto.VehicleId} nie istnieje." });
            }

            var log = new TelemetryLog
            {
                VehicleId = dto.VehicleId,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                SpeedKph = dto.SpeedKph,
                FuelLevel = dto.FuelLevel,
                Timestamp = dto.Timestamp
            };

            _context.TelemetryLogs.Add(log);
            await _context.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetTelemetry), new { id = log.Id }, new { Id = log.Id });
        }

        // POST: api/Telemetry/Simulate/{vehicleId}?count=10
        // Generuje symulowane wpisy telemetryczne dla testów
        [HttpPost("Simulate/{vehicleId}")]
        public async Task<IActionResult> SimulateTelemetry(int vehicleId, [FromQuery] int count = 10, CancellationToken ct = default)
        {
            var logs = await _telemetryService.SimulateTelemetryAsync(vehicleId, count, ct);
            var dtos = logs.Select(l => new TelemetryLogReadDto
            {
                Id = l.Id,
                VehicleId = l.VehicleId,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                SpeedKph = l.SpeedKph,
                FuelLevel = l.FuelLevel,
                Timestamp = l.Timestamp,
                RowVersion = l.RowVersion
            });

            return Ok(dtos);
        }
        // GET: api/Telemetry/Vehicle/{vehicleId}
        [HttpGet("Vehicle/{vehicleId}")]
        public async Task<ActionResult<IEnumerable<TelemetryLogReadDto>>> GetVehicleTelemetry(int vehicleId, CancellationToken ct)
        {
            var logs = await _context.TelemetryLogs
                .Where(t => t.VehicleId == vehicleId)
                .OrderByDescending(t => t.Timestamp)
                .Select(t => new TelemetryLogReadDto
                {
                    Id = t.Id,
                    VehicleId = t.VehicleId,
                    Latitude = t.Latitude,
                    Longitude = t.Longitude,
                    SpeedKph = t.SpeedKph,
                    FuelLevel = t.FuelLevel,
                    Timestamp = t.Timestamp,
                    RowVersion = t.RowVersion
                })
                .ToListAsync(ct);

            return Ok(logs);
        }

        // GET: api/Telemetry/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TelemetryLogReadDto>> GetTelemetry(int id, CancellationToken ct)
        {
            var dto = await _context.TelemetryLogs
                .Where(t => t.Id == id)
                .Select(t => new TelemetryLogReadDto
                {
                    Id = t.Id,
                    VehicleId = t.VehicleId,
                    Latitude = t.Latitude,
                    Longitude = t.Longitude,
                    SpeedKph = t.SpeedKph,
                    FuelLevel = t.FuelLevel,
                    Timestamp = t.Timestamp,
                    RowVersion = t.RowVersion
                })
                .FirstOrDefaultAsync(ct);

            if (dto == null) return NotFound();
            return Ok(dto);
        }
    }
*/
}
