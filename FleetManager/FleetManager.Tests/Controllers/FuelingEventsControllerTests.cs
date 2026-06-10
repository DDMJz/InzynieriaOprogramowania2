using System.Net;
using System.Net.Http.Json;
using FleetManager.DTOs;
using FleetManager.Tests.Helpers;

namespace FleetManager.Tests.Controllers;

public class FuelingEventsControllerTests : IClassFixture<FleetManagerFactory>
{
    private readonly HttpClient _client;

    public FuelingEventsControllerTests(FleetManagerFactory factory)
    {
        factory.EnsureDb();
        _client = factory.CreateClient();
    }

    private static string NewVin() => Guid.NewGuid().ToString("N")[..17].ToUpper();

    private async Task<int> CreateVehicleAsync(int odometer = 0)
    {
        var payload = new { vin = NewVin(), licensePlate = "F01", brand = "Brand", model = "Model", year = 2020, fuelTankCapacity = 50.0, fuelConsumption = 8.0, odometerReading = odometer };
        var resp = await _client.PostAsJsonAsync("/api/vehicles", payload);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<VehicleCreatedResponseDto>())!.Id;
    }

    private async Task<int> CreateFuelingEventAsync(
        int vehicleId,
        int odometer = 100,
        double liters = 30.0,
        double cost = 150.0,
        DateTime? date = null)
    {
        var payload = new
        {
            vehicleId,
            litersAdded = liters,
            totalCost = cost,
            odometerReading = odometer,
            date = date ?? DateTime.UtcNow
        };
        var resp = await _client.PostAsJsonAsync("/api/fuelingevents", payload);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<FuelingEventCreatedResponseDto>())!.Id;
    }

    // --- GET /api/fuelingevents/Vehicle/{vehicleId} ---

    [Fact]
    public async Task GetVehicleHistory_Returns200WithEmptyList_ForNewVehicle()
    {
        var vehicleId = await CreateVehicleAsync();

        var response = await _client.GetAsync($"/api/fuelingevents/Vehicle/{vehicleId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<FuelingEventReadDto>>();
        Assert.NotNull(list);
        Assert.Empty(list);
    }

    [Fact]
    public async Task GetVehicleHistory_ReturnsEventsOrderedByDateDesc()
    {
        var vehicleId = await CreateVehicleAsync();
        var dateOld = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var dateNew = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        await CreateFuelingEventAsync(vehicleId, odometer: 100, date: dateOld);
        await CreateFuelingEventAsync(vehicleId, odometer: 200, date: dateNew);

        var response = await _client.GetAsync($"/api/fuelingevents/Vehicle/{vehicleId}");
        var list = await response.Content.ReadFromJsonAsync<List<FuelingEventReadDto>>();

        Assert.NotNull(list);
        Assert.Equal(2, list.Count);
        Assert.Equal(200, list[0].OdometerReading); // newer (June) first
        Assert.Equal(100, list[1].OdometerReading);
    }

    // --- GET /api/fuelingevents/{id} ---

    [Fact]
    public async Task GetFuelingEvent_Returns200_WhenFound()
    {
        var vehicleId = await CreateVehicleAsync();
        var eventId = await CreateFuelingEventAsync(vehicleId);

        var response = await _client.GetAsync($"/api/fuelingevents/{eventId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<FuelingEventReadDto>();
        Assert.NotNull(dto);
        Assert.Equal(eventId, dto.Id);
        Assert.Equal(vehicleId, dto.VehicleId);
    }

    [Fact]
    public async Task GetFuelingEvent_Returns404_WhenNotFound()
    {
        var response = await _client.GetAsync("/api/fuelingevents/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- POST /api/fuelingevents ---

    [Fact]
    public async Task PostFuelingEvent_Returns201_WithValidData()
    {
        var vehicleId = await CreateVehicleAsync();
        var payload = new
        {
            vehicleId,
            litersAdded = 45.0,
            totalCost = 225.0,
            odometerReading = 500,
            date = DateTime.UtcNow
        };

        var response = await _client.PostAsJsonAsync("/api/fuelingevents", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<FuelingEventCreatedResponseDto>();
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task PostFuelingEvent_Returns404_WhenVehicleNotFound()
    {
        var payload = new
        {
            vehicleId = 999999,
            litersAdded = 30.0,
            totalCost = 150.0,
            odometerReading = 100,
            date = DateTime.UtcNow
        };

        var response = await _client.PostAsJsonAsync("/api/fuelingevents", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostFuelingEvent_Returns400_WhenLitersAddedIsZero()
    {
        var vehicleId = await CreateVehicleAsync();
        var payload = new
        {
            vehicleId,
            litersAdded = 0.0,
            cost = 0.0,
            odometerReading = 100,
            date = DateTime.UtcNow
        };

        var response = await _client.PostAsJsonAsync("/api/fuelingevents", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostFuelingEvent_UpdatesVehicleOdometer()
    {
        var vehicleId = await CreateVehicleAsync(odometer: 0);
        var payload = new
        {
            vehicleId,
            litersAdded = 40.0,
            cost = 200.0,
            odometerReading = 1500,
            date = DateTime.UtcNow
        };
        await _client.PostAsJsonAsync("/api/fuelingevents", payload);

        var vehicleResp = await _client.GetAsync($"/api/vehicles/{vehicleId}");
        var vehicle = await vehicleResp.Content.ReadFromJsonAsync<VehicleReadDto>();

        Assert.NotNull(vehicle);
        Assert.Equal(1500, vehicle.OdometerReading);
    }

    // --- DELETE /api/fuelingevents/{id} ---

    [Fact]
    public async Task DeleteFuelingEvent_Returns204_WhenDeleted()
    {
        var vehicleId = await CreateVehicleAsync();
        var eventId = await CreateFuelingEventAsync(vehicleId);

        var response = await _client.DeleteAsync($"/api/fuelingevents/{eventId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteFuelingEvent_Returns404_WhenNotFound()
    {
        var response = await _client.DeleteAsync("/api/fuelingevents/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
