
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FleetManager.Data;
using FleetManager.Models;

namespace FleetManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class MaintenanceTypesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MaintenanceTypesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MaintenanceType>>> GetTypes(CancellationToken ct)
        {
            return await _context.MaintenanceTypes.ToListAsync(ct);
        }
    }
}
