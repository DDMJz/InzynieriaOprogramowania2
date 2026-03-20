using FleetManager.Data;
using FleetManager.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetManager.Services
{
    // Prosty serwis symulujący dane telemetryczne (może być użyty do testów)
    public class TelemetryService : ITelemetryService
    {
        private readonly AppDbContext _context;
        private readonly Random _rng = new Random();

        public TelemetryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TelemetryLog>> SimulateTelemetryAsync(int vehicleId, int count, CancellationToken ct)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId, ct);
            if (vehicle == null) return Enumerable.Empty<TelemetryLog>();

            var logs = new List<TelemetryLog>();
            for (int i = 0; i < count; i++)
            {
                var lat = vehicle.LastKnownLatitude ?? (50 + _rng.NextDouble());
                var lon = vehicle.LastKnownLongitude ?? (19 + _rng.NextDouble());
                var speed = _rng.NextDouble() * 120; // do 120 kph
                var fuel = Math.Max(0, vehicle.CurrentFuelLevel - _rng.NextDouble() * 2);

                var log = new TelemetryLog
                {
                    VehicleId = vehicleId,
                    Latitude = lat + (_rng.NextDouble() - 0.5) * 0.01,
                    Longitude = lon + (_rng.NextDouble() - 0.5) * 0.01,
                    SpeedKph = Math.Round(speed, 2),
                    FuelLevel = Math.Round(fuel, 2),
                    Timestamp = DateTime.UtcNow.AddSeconds(-i * 10)
                };

                logs.Add(log);
            }

            // Zapis do bazy
            _context.TelemetryLogs.AddRange(logs);
            await _context.SaveChangesAsync(ct);

            // Aktualizacja ostatniej znanej pozycji pojazdu
            var newest = logs.OrderByDescending(l => l.Timestamp).First();
            vehicle.LastKnownLatitude = newest.Latitude;
            vehicle.LastKnownLongitude = newest.Longitude;
            vehicle.LastGpsUpdate = newest.Timestamp;
            await _context.SaveChangesAsync(ct);

            return logs;
        }
    }
}
