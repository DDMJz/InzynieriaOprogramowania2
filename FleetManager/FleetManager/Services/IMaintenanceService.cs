using FleetManager.DTOs;
using FleetManager.Models;

namespace FleetManager.Services
{
    // Serwis eksploatacji (maintenance): reguły dla zdarzeń serwisowych
    // Zawiera podstawowe operacje biznesowe: tworzenie oraz usuwanie zdarzeń serwisowych.
    // Implementacja powinna: walidować dane wejściowe, aktualizować stan pojazdu (przebieg),
    // oraz odwracać efekty zdarzenia przy jego usunięciu.
    public interface IMaintenanceService
    {
        // Tworzy zdarzenie serwisowe i aktualizuje przebieg pojazdu.
        // Zwracany tuple:
        //  - Success: czy operacja powiodła się
        //  - Error: opcjonalny komunikat błędu (np. walidacja, brak pojazdu)
        //  - Event: utworzony obiekt MaintenanceEvent (w przypadku sukcesu)
        // CancellationToken umożliwia przerwanie operacji podczas zapisów do bazy.
        Task<(bool Success, string? Error, MaintenanceEvent? Event)> CreateMaintenanceAsync(MaintenanceEventCreateDto dto, CancellationToken ct);

        // Usuwa zdarzenie serwisowe i cofa jego wpływ na przebieg pojazdu.
        // Zwracany tuple: Success oraz opcjonalny komunikat Error.
        // Implementacja powinna bezpiecznie obsłużyć brak rekordu i zachować spójność danych.
        Task<(bool Success, string? Error)> DeleteMaintenanceAsync(int id, CancellationToken ct);
    }
}
