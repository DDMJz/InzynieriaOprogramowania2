using FleetManager.Models;

namespace FleetManager.Tests.Models;

public class VehicleModelTests
{
    [Fact]
    public void Vehicle_DefaultStatus_IsIdle()
    {
        var vehicle = new Vehicle();

        Assert.Equal(VehicleStatus.Idle, vehicle.Status);
    }

    [Fact]
    public void Vehicle_DefaultRowVersion_IsEmpty()
    {
        var vehicle = new Vehicle();

        Assert.NotNull(vehicle.RowVersion);
        Assert.Empty(vehicle.RowVersion);
    }

    [Fact]
    public void Vehicle_DefaultOdometerReading_IsZero()
    {
        var vehicle = new Vehicle();

        Assert.Equal(0, vehicle.OdometerReading);
    }
}
