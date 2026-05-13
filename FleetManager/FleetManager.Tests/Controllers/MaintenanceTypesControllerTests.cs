using System.Net;
using System.Net.Http.Json;
using FleetManager.Models;
using FleetManager.Tests.Helpers;

namespace FleetManager.Tests.Controllers;

public class MaintenanceTypesControllerTests : IClassFixture<FleetManagerFactory>
{
    private readonly HttpClient _client;

    public MaintenanceTypesControllerTests(FleetManagerFactory factory)
    {
        factory.EnsureDb();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTypes_Returns200()
    {
        var response = await _client.GetAsync("/api/maintenancetypes");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTypes_ReturnsAllThreeSeededTypes()
    {
        var response = await _client.GetAsync("/api/maintenancetypes");
        var types = await response.Content.ReadFromJsonAsync<List<MaintenanceType>>();

        Assert.NotNull(types);
        Assert.Equal(3, types.Count);
        Assert.Contains(types, t => t.Name == "Wymiana Oleju" && t.DefaultIntervalOdometer == 15000);
        Assert.Contains(types, t => t.Name == "Przegląd Rejestracyjny" && t.DefaultIntervalDays == 365);
        Assert.Contains(types, t => t.Name == "Inne" && t.DefaultIntervalOdometer == 0 && t.DefaultIntervalDays == 0);
    }
}
