
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FleetManager.Data;
using FleetManager.DTOs;

namespace FleetManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class MaintenanceTypesController : ApiBaseController
    {
        private readonly AppDbContext _context;

        public MaintenanceTypesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MaintenanceTypeReadDto>>> GetTypes(CancellationToken ct)
        {
            var types = await _context.MaintenanceTypes
                .Select(t => new MaintenanceTypeReadDto(
                    t.Id,
                    t.Name,
                    t.SystemCode,
                    t.DefaultIntervalOdometer,
                    t.DefaultIntervalDays
            ))
            .ToListAsync(ct);

            return Ok(types);
        }
    }
}
