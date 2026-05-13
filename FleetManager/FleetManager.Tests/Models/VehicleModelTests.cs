using FleetManager.Models;

namespace FleetManager.Tests.Models;

public class VehicleModelTests
{
    [Fact]
    public void FuelLevelPercentage_ReturnsCorrectValue()
    {
        var vehicle = new Vehicle { FuelTankCapacity = 100, CurrentFuelLevel = 50 };

        Assert.Equal(50.0, vehicle.FuelLevelPercentage);
    }

    [Fact]
    public void FuelLevelPercentage_ReturnsZero_WhenTankCapacityIsZero()
    {
        var vehicle = new Vehicle { FuelTankCapacity = 0, CurrentFuelLevel = 0 };

        Assert.Equal(0.0, vehicle.FuelLevelPercentage);
    }

    [Fact]
    public void FuelLevelPercentage_RoundsToTwoDecimalPlaces()
    {
        // 1/3 * 100 = 33.3333... → rounded to 33.33
        var vehicle = new Vehicle { FuelTankCapacity = 3, CurrentFuelLevel = 1 };

        Assert.Equal(33.33, vehicle.FuelLevelPercentage);
    }

    [Fact]
    public void FuelLevelPercentage_ReturnsHundred_WhenFull()
    {
        var vehicle = new Vehicle { FuelTankCapacity = 60, CurrentFuelLevel = 60 };

        Assert.Equal(100.0, vehicle.FuelLevelPercentage);
    }
}
