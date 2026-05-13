using FleetManager.DTOs;
using FleetManager.Models;
using FleetManager.Common.Results;

namespace FleetManager.Services
{
    
    public interface IMaintenanceService
    {
        // Tworzy zdarzenie serwisowe i aktualizuje przebieg pojazdu.
        // Zwraca: obiekt Result z typem błędu (NotFound/Validation) lub obiektem dziedziny.
        // CancellationToken umożliwia przerwanie operacji podczas zapisów do bazy.
        Task<Result<MaintenanceEvent>> CreateMaintenanceAsync(MaintenanceEventCreateDto dto, CancellationToken ct);

        // Usuwa zdarzenie serwisowe 
        // Zwraca: obiekt Result bez ładunku danych lub bledem NotFound.
        Task<Result> DeleteMaintenanceAsync(int id, CancellationToken ct);
    }
}
