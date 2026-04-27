using FleetManager.Data;
using FleetManager.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetManager.Workers
{
    public class TelemetrySimulatorWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TelemetrySimulatorWorker> _logger;

        public TelemetrySimulatorWorker(IServiceProvider serviceProvider, ILogger<TelemetrySimulatorWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation("Demon Symulacji Ruchu zainicjowany");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    // using-odcięcie pamięci bazy danych po wykonaniu mutacji
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        // pobieranie z bazy do pamieci tylko pojazdow o stusie InTransit
                        var activeVehicles = await context.Vehicles
                            .Where(v => v.Status == VehicleStatus.InTransit)
                            .ToListAsync(ct);

                        if (activeVehicles.Any())
                        {
                            foreach (var vehicle in activeVehicles)
                            {
                                // przyrost przebiegu o 1 (przy okresie 40sek to daje 90km/h)
                                vehicle.OdometerReading += 1;
                            }

                            await context.SaveChangesAsync(ct);

                            _logger.LogInformation(
                                "Symulator ruchu: Naliczono nowy przebieg dla {Count} pojazdów.",
                                activeVehicles.Count);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // zerwane placzenie z baza nie konczy pracy demona - wyjatek jest wypisywany przez logger
                    _logger.LogError(ex, "Wystąpił błąd podczas pracy Demona Symulacji.");
                }

                // opoznienie 40 sekund
                await Task.Delay(40_000, ct);
            }
        }
    }
}