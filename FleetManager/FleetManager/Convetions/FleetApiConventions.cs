using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace FleetManager.Conventions
{
    public static class FleetApiConventions
    {
        // 1. GET: Pobieranie kolekcji (np. GetVehicles)
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        public static void Get(
            // dopasowanie do CancellationToken i ewentualnych innych parametrow
            [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)] CancellationToken ct = default)
        { }

        // 2. GET: Pobieranie jednego elementu (np. GetVehicle)
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        public static void Get(
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)] int id,
            [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)] CancellationToken ct = default)
        { }

        // 3. POST: Tworzenie zasobu (np. PostVehicle)
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        public static void Post(
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)] object model,
            [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)] CancellationToken ct = default)
        { }

        // 4. PUT: Aktualizacja zasobu (np. PutVehicle)
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        public static void Put(
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)] int id,
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)] object model,
            [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)] CancellationToken ct = default)
        { }

        // 5. DELETE: Usuwanie zasobu (np. DeleteVehicle)
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        public static void Delete(
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)] int id,
            [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)] CancellationToken ct = default)
        { }
    }
}