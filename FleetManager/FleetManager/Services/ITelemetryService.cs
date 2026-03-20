using FleetManager.Models;

namespace FleetManager.Services
{
    // Serwis do symulacji i przetwarzania logów telemetrycznych
    public interface ITelemetryService
    {
        // Generuje "count" symulowanych wpisów telemetrycznych dla pojazdu
        Task<IEnumerable<TelemetryLog>> SimulateTelemetryAsync(int vehicleId, int count, CancellationToken ct);
    }
}
