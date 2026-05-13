using System.Net;
using System.Net.Http.Json;
using FleetManager.DTOs;
using FleetManager.Tests.Helpers;

namespace FleetManager.Tests.Controllers;

public class VehiclesControllerTests : IClassFixture<FleetManagerFactory>
{
    private readonly HttpClient _client;

    public VehiclesControllerTests(FleetManagerFactory factory)
    {
        factory.EnsureDb();
        _client = factory.CreateClient();
    }

    private static string NewVin() => Guid.NewGuid().ToString("N")[..17].ToUpper();

    private async Task<int> CreateVehicleAsync(string? vin = null)
    {
        var payload = new
        {
            vin = vin ?? NewVin(),
            licensePlate = "TST001",
            brand = "TestBrand",
            model = "TestModel",
            odometerReading = 0
        };
        var response = await _client.PostAsJsonAsync("/api/vehicles", payload);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<VehicleCreatedResponseDto>();
        return result!.Id;
    }

    // --- GET /api/vehicles ---

    [Fact]
    public async Task GetVehicles_Returns200()
    {
        var response = await _client.GetAsync("/api/vehicles");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetVehicles_ContainsCreatedVehicle()
    {
        var id = await CreateVehicleAsync();

        var response = await _client.GetAsync("/api/vehicles");
        var vehicles = await response.Content.ReadFromJsonAsync<List<VehicleReadDto>>();

        Assert.NotNull(vehicles);
        Assert.Contains(vehicles, v => v.Id == id);
    }

    // --- GET /api/vehicles/{id} ---

    [Fact]
    public async Task GetVehicle_Returns200_WhenFound()
    {
        var id = await CreateVehicleAsync();

        var response = await _client.GetAsync($"/api/vehicles/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<VehicleReadDto>();
        Assert.NotNull(dto);
        Assert.Equal(id, dto.Id);
    }

    [Fact]
    public async Task GetVehicle_Returns404_WhenNotFound()
    {
        var response = await _client.GetAsync("/api/vehicles/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- POST /api/vehicles ---

    [Fact]
    public async Task PostVehicle_Returns201_WithValidData()
    {
        var payload = new { vin = NewVin(), licensePlate = "NEW001", brand = "BMW", model = "X5", odometerReading = 1000 };

        var response = await _client.PostAsJsonAsync("/api/vehicles", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<VehicleCreatedResponseDto>();
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task PostVehicle_Returns400_WhenVinDuplicate()
    {
        var vin = NewVin();
        await CreateVehicleAsync(vin);

        var payload = new { vin, licensePlate = "DUP001", brand = "Ford", model = "Focus", odometerReading = 0 };
        var response = await _client.PostAsJsonAsync("/api/vehicles", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostVehicle_Returns400_WhenVinTooShort()
    {
        var payload = new { vin = "SHORT", licensePlate = "ABC001", brand = "Brand", model = "Model", odometerReading = 0 };

        var response = await _client.PostAsJsonAsync("/api/vehicles", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // --- PUT /api/vehicles/{id} ---

    [Fact]
    public async Task PutVehicle_Returns204_WhenUpdated()
    {
        var id = await CreateVehicleAsync();
        var getResp = await _client.GetAsync($"/api/vehicles/{id}");
        var existing = await getResp.Content.ReadFromJsonAsync<VehicleReadDto>();

        var payload = new
        {
            licensePlate = "UPD001",
            brand = "Honda",
            model = "Civic",
            year = 2022,
            rowVersion = existing!.RowVersion
        };
        var response = await _client.PutAsJsonAsync($"/api/vehicles/{id}", payload);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task PutVehicle_Returns404_WhenNotFound()
    {
        var payload = new { licensePlate = "NF001", brand = "Honda", model = "Civic", year = 2022, rowVersion = Array.Empty<byte>() };

        var response = await _client.PutAsJsonAsync("/api/vehicles/999999", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutVehicle_Returns409_OnConcurrencyConflict()
    {
        var id = await CreateVehicleAsync();
        // Send a RowVersion that does not match the stored empty byte array
        var payload = new
        {
            licensePlate = "CNF001",
            brand = "Honda",
            model = "Civic",
            year = 2022,
            rowVersion = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }
        };

        var response = await _client.PutAsJsonAsync($"/api/vehicles/{id}", payload);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // --- DELETE /api/vehicles/{id} ---

    [Fact]
    public async Task DeleteVehicle_Returns204_WhenDeleted()
    {
        var id = await CreateVehicleAsync();

        var response = await _client.DeleteAsync($"/api/vehicles/{id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteVehicle_Returns404_WhenNotFound()
    {
        var response = await _client.DeleteAsync("/api/vehicles/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
