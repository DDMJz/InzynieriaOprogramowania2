using System.Net;
using System.Net.Http.Json;
using FleetManager.DTOs;
using FleetManager.Tests.Helpers;

namespace FleetManager.Tests.Controllers;

public class MaintenanceEventsControllerTests : IClassFixture<FleetManagerFactory>
{
    private readonly HttpClient _client;
    private const int OilChangeTypeId = 1;   // seeded: "Wymiana Oleju"
    private const int InspectionTypeId = 2;  // seeded: "Przegląd Rejestracyjny"

    public MaintenanceEventsControllerTests(FleetManagerFactory factory)
    {
        factory.EnsureDb();
        _client = factory.CreateClient();
    }

    private static string NewVin() => Guid.NewGuid().ToString("N")[..17].ToUpper();

    private async Task<int> CreateVehicleAsync(int odometer = 0)
    {
        var payload = new { vin = NewVin(), licensePlate = "M01", brand = "Brand", model = "Model", year = 2020, fuelTankCapacity = 50.0, fuelConsumption = 8.0, odometerReading = odometer };
        var resp = await _client.PostAsJsonAsync("/api/vehicles", payload);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<VehicleCreatedResponseDto>())!.Id;
    }

    private async Task<int> CreateMaintenanceEventAsync(
        int vehicleId,
        int odometer = 100,
        int typeId = OilChangeTypeId,
        double cost = 200.0,
        string description = "Test maintenance")
    {
        var payload = new
        {
            vehicleId,
            maintenanceTypeId = typeId,
            odometerReading = odometer,
            totalCost = cost,
            description,
            date = DateTime.UtcNow
        };
        var resp = await _client.PostAsJsonAsync("/api/maintenanceevents", payload);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<MaintenanceEventCreatedResponseDto>())!.Id;
    }

    // --- GET /api/maintenanceevents/Vehicle/{vehicleId} ---

    [Fact]
    public async Task GetVehicleHistory_Returns200WithEmptyList_ForNewVehicle()
    {
        var vehicleId = await CreateVehicleAsync();

        var response = await _client.GetAsync($"/api/maintenanceevents/Vehicle/{vehicleId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<MaintenanceEventReadDto>>();
        Assert.NotNull(list);
        Assert.Empty(list);
    }

    [Fact]
    public async Task GetVehicleHistory_ReturnsEventWithMaintenanceTypeName()
    {
        var vehicleId = await CreateVehicleAsync();
        await CreateMaintenanceEventAsync(vehicleId, odometer: 500, typeId: OilChangeTypeId);

        var response = await _client.GetAsync($"/api/maintenanceevents/Vehicle/{vehicleId}");
        var list = await response.Content.ReadFromJsonAsync<List<MaintenanceEventReadDto>>();

        Assert.NotNull(list);
        Assert.Single(list);
        Assert.Equal("Wymiana Oleju", list[0].MaintenanceTypeName);
        Assert.Equal(vehicleId, list[0].VehicleId);
    }

    // --- GET /api/maintenanceevents/{id} ---

    [Fact]
    public async Task GetMaintenanceEvent_Returns200_WhenFound()
    {
        var vehicleId = await CreateVehicleAsync();
        var eventId = await CreateMaintenanceEventAsync(vehicleId);

        var response = await _client.GetAsync($"/api/maintenanceevents/{eventId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<MaintenanceEventReadDto>();
        Assert.NotNull(dto);
        Assert.Equal(eventId, dto.Id);
        Assert.Equal(vehicleId, dto.VehicleId);
    }

    [Fact]
    public async Task GetMaintenanceEvent_Returns404_WhenNotFound()
    {
        var response = await _client.GetAsync("/api/maintenanceevents/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- POST /api/maintenanceevents ---

    [Fact]
    public async Task PostMaintenanceEvent_Returns201_WithValidData()
    {
        var vehicleId = await CreateVehicleAsync();
        var payload = new
        {
            vehicleId,
            maintenanceTypeId = OilChangeTypeId,
            odometerReading = 15000,
            totalCost = 250.0,
            description = "Oil change at 15000 km",
            date = DateTime.UtcNow
        };

        var response = await _client.PostAsJsonAsync("/api/maintenanceevents", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<MaintenanceEventCreatedResponseDto>();
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task PostMaintenanceEvent_Returns404_WhenVehicleNotFound()
    {
        var payload = new
        {
            vehicleId = 999999,
            maintenanceTypeId = OilChangeTypeId,
            odometerReading = 100,
            totalCost = 200.0,
            description = "Test",
            date = DateTime.UtcNow
        };

        var response = await _client.PostAsJsonAsync("/api/maintenanceevents", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostMaintenanceEvent_Returns404_WhenMaintenanceTypeNotFound()
    {
        var vehicleId = await CreateVehicleAsync();
        var payload = new
        {
            vehicleId,
            maintenanceTypeId = 9999, // doesn't exist
            odometerReading = 100,
            totalCost = 200.0,
            description = "Test",
            date = DateTime.UtcNow
        };

        var response = await _client.PostAsJsonAsync("/api/maintenanceevents", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostMaintenanceEvent_Returns400_WhenCostIsNegative()
    {
        var vehicleId = await CreateVehicleAsync();
        var payload = new
        {
            vehicleId,
            maintenanceTypeId = OilChangeTypeId,
            odometerReading = 100,
            totalCost = -50.0,
            description = "Test",
            date = DateTime.UtcNow
        };

        var response = await _client.PostAsJsonAsync("/api/maintenanceevents", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostMaintenanceEvent_UpdatesVehicleOdometer()
    {
        var vehicleId = await CreateVehicleAsync(odometer: 0);
        var payload = new
        {
            vehicleId,
            maintenanceTypeId = InspectionTypeId,
            odometerReading = 20000,
            totalCost = 300.0,
            description = "Annual inspection",
            date = DateTime.UtcNow
        };
        await _client.PostAsJsonAsync("/api/maintenanceevents", payload);

        var vehicleResp = await _client.GetAsync($"/api/vehicles/{vehicleId}");
        var vehicle = await vehicleResp.Content.ReadFromJsonAsync<VehicleReadDto>();

        Assert.NotNull(vehicle);
        Assert.Equal(20000, vehicle.OdometerReading);
    }

    // --- DELETE /api/maintenanceevents/{id} ---

    [Fact]
    public async Task DeleteMaintenanceEvent_Returns204_WhenDeleted()
    {
        var vehicleId = await CreateVehicleAsync();
        var eventId = await CreateMaintenanceEventAsync(vehicleId);

        var response = await _client.DeleteAsync($"/api/maintenanceevents/{eventId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMaintenanceEvent_Returns404_WhenNotFound()
    {
        var response = await _client.DeleteAsync("/api/maintenanceevents/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
