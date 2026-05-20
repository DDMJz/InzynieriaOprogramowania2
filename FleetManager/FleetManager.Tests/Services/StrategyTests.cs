using FleetManager.Models;
using FleetManager.Strategies;

namespace FleetManager.Tests.Services;

public class DistanceOnlyStrategyTests
{
    private readonly DistanceOnlyStrategy _strategy = new();

    private static Vehicle NewVehicle(int odometer = 0) =>
        new Vehicle { OdometerReading = odometer };

    private static MaintenanceType OdometerOnlyType(int km = 30000) =>
        new MaintenanceType { DefaultIntervalOdometer = km, DefaultIntervalDays = null };

    private static MaintenanceType BothIntervalsType() =>
        new MaintenanceType { DefaultIntervalOdometer = 15000, DefaultIntervalDays = 365 };

    [Fact]
    public void CanHandle_ReturnsTrue_WhenOnlyOdometerInterval()
    {
        Assert.True(_strategy.CanHandle(OdometerOnlyType()));
    }

    [Fact]
    public void CanHandle_ReturnsFalse_WhenBothIntervals()
    {
        Assert.False(_strategy.CanHandle(BothIntervalsType()));
    }

    [Fact]
    public void Evaluate_ReturnsOk_WhenFarFromLimit()
    {
        var ctx = new MaintenanceEvaluationContext(NewVehicle(1000), OdometerOnlyType(30000), null, 0, 30000, null);

        var result = _strategy.Evaluate(ctx);

        Assert.Equal(MaintenanceStatusLevel.Ok, result.Level);
        Assert.Equal(29000, result.KilometersRemaining);
    }

    [Fact]
    public void Evaluate_ReturnsWarning_WhenWithin1000km()
    {
        // 29500 driven, 30000 interval → 500 remaining (within 1000 threshold)
        var ctx = new MaintenanceEvaluationContext(NewVehicle(29500), OdometerOnlyType(30000), null, 0, 30000, null);

        var result = _strategy.Evaluate(ctx);

        Assert.Equal(MaintenanceStatusLevel.Warning, result.Level);
        Assert.Equal(500, result.KilometersRemaining);
    }

    [Fact]
    public void Evaluate_ReturnsCritical_WhenLimitExceeded()
    {
        // 31000 driven, 30000 interval → -1000
        var ctx = new MaintenanceEvaluationContext(NewVehicle(31000), OdometerOnlyType(30000), null, 0, 30000, null);

        var result = _strategy.Evaluate(ctx);

        Assert.Equal(MaintenanceStatusLevel.Critical, result.Level);
        Assert.True(result.KilometersRemaining!.Value <= 0);
    }

    [Fact]
    public void Evaluate_UsesLastEventOdometer_AsBaseline()
    {
        var vehicle = NewVehicle(20000);
        var lastEvent = new MaintenanceEvent { OdometerReading = 10000 };
        // driven since last: 10000, remaining: 30000 - 10000 = 20000
        var ctx = new MaintenanceEvaluationContext(vehicle, OdometerOnlyType(30000), lastEvent, 0, 30000, null);

        var result = _strategy.Evaluate(ctx);

        Assert.Equal(MaintenanceStatusLevel.Ok, result.Level);
        Assert.Equal(20000, result.KilometersRemaining);
    }

    [Fact]
    public void Evaluate_Throws_WhenNoOdometerInterval()
    {
        var ctx = new MaintenanceEvaluationContext(NewVehicle(1000), OdometerOnlyType(), null, 0, null, null);

        Assert.Throws<InvalidOperationException>(() => _strategy.Evaluate(ctx));
    }
}

public class TimeOnlyStrategyTests
{
    private readonly TimeOnlyStrategy _strategy = new();

    private static Vehicle NewVehicle(int createdDaysAgo = 0) =>
        new Vehicle { CreatedAt = DateTime.UtcNow.AddDays(-createdDaysAgo) };

    private static MaintenanceType DaysOnlyType(int days = 365) =>
        new MaintenanceType { DefaultIntervalOdometer = null, DefaultIntervalDays = days };

    private static MaintenanceType BothIntervalsType() =>
        new MaintenanceType { DefaultIntervalOdometer = 15000, DefaultIntervalDays = 365 };

    [Fact]
    public void CanHandle_ReturnsTrue_WhenOnlyDaysInterval()
    {
        Assert.True(_strategy.CanHandle(DaysOnlyType()));
    }

    [Fact]
    public void CanHandle_ReturnsFalse_WhenBothIntervals()
    {
        Assert.False(_strategy.CanHandle(BothIntervalsType()));
    }

    [Fact]
    public void Evaluate_ReturnsOk_WhenFarFromDue()
    {
        // Created 10 days ago, interval 365 → 355 remaining
        var ctx = new MaintenanceEvaluationContext(NewVehicle(10), DaysOnlyType(365), null, 0, null, 365);

        var result = _strategy.Evaluate(ctx);

        Assert.Equal(MaintenanceStatusLevel.Ok, result.Level);
        Assert.True(result.DaysRemaining > 14);
    }

    [Fact]
    public void Evaluate_ReturnsWarning_WhenWithin14Days()
    {
        // Created 358 days ago, interval 365 → ~7 days remaining
        var ctx = new MaintenanceEvaluationContext(NewVehicle(358), DaysOnlyType(365), null, 0, null, 365);

        var result = _strategy.Evaluate(ctx);

        Assert.Equal(MaintenanceStatusLevel.Warning, result.Level);
    }

    [Fact]
    public void Evaluate_ReturnsCritical_WhenOverdue()
    {
        // Created 400 days ago, interval 365 → overdue
        var ctx = new MaintenanceEvaluationContext(NewVehicle(400), DaysOnlyType(365), null, 0, null, 365);

        var result = _strategy.Evaluate(ctx);

        Assert.Equal(MaintenanceStatusLevel.Critical, result.Level);
        Assert.True(result.DaysRemaining!.Value <= 0);
    }

    [Fact]
    public void Evaluate_UsesLastEventDate_AsBaseline()
    {
        // Vehicle created 400 days ago (overdue), but last event 10 days ago → ok
        var vehicle = NewVehicle(400);
        var lastEvent = new MaintenanceEvent { Date = DateTime.UtcNow.AddDays(-10) };
        var ctx = new MaintenanceEvaluationContext(vehicle, DaysOnlyType(365), lastEvent, 0, null, 365);

        var result = _strategy.Evaluate(ctx);

        Assert.Equal(MaintenanceStatusLevel.Ok, result.Level);
    }

    [Fact]
    public void Evaluate_Throws_WhenNoDaysInterval()
    {
        var ctx = new MaintenanceEvaluationContext(NewVehicle(10), DaysOnlyType(), null, 0, null, null);

        Assert.Throws<InvalidOperationException>(() => _strategy.Evaluate(ctx));
    }
}

public class CompositeStrategyTests
{
    private readonly CompositeStrategy _strategy = new(new TimeOnlyStrategy(), new DistanceOnlyStrategy());

    private static Vehicle NewVehicle(int odometer = 0, int createdDaysAgo = 10, double fuelConsumption = 10) =>
        new Vehicle
        {
            OdometerReading = odometer,
            CreatedAt = DateTime.UtcNow.AddDays(-createdDaysAgo),
            FuelConsumption = fuelConsumption
        };

    private static MaintenanceType BothIntervalsType(int km = 15000, int days = 365) =>
        new MaintenanceType { DefaultIntervalOdometer = km, DefaultIntervalDays = days };

    private static MaintenanceType OdometerOnlyType() =>
        new MaintenanceType { DefaultIntervalOdometer = 30000, DefaultIntervalDays = null };

    [Fact]
    public void CanHandle_ReturnsTrue_WhenBothIntervals()
    {
        Assert.True(_strategy.CanHandle(BothIntervalsType()));
    }

    [Fact]
    public void CanHandle_ReturnsFalse_WhenOnlyOdometer()
    {
        Assert.False(_strategy.CanHandle(OdometerOnlyType()));
    }

    [Fact]
    public void Evaluate_ReturnsOk_WhenBothDimensionsOk()
    {
        var vehicle = NewVehicle(odometer: 1000, createdDaysAgo: 10);
        var ctx = new MaintenanceEvaluationContext(vehicle, BothIntervalsType(15000, 365), null, 0, 15000, 365);

        var result = _strategy.Evaluate(ctx);

        Assert.Equal(MaintenanceStatusLevel.Ok, result.Level);
    }

    [Fact]
    public void Evaluate_ReturnsCritical_WhenDistanceExceeded()
    {
        var vehicle = NewVehicle(odometer: 16000, createdDaysAgo: 10);
        var ctx = new MaintenanceEvaluationContext(vehicle, BothIntervalsType(15000, 365), null, 0, 15000, 365);

        var result = _strategy.Evaluate(ctx);

        Assert.Equal(MaintenanceStatusLevel.Critical, result.Level);
    }

    [Fact]
    public void Evaluate_AddsNoteAndReducesIntervals_WhenSevereUsage()
    {
        // FuelConsumption=10, threshold=120%=12L, fuel consumed=14L → severe
        var vehicle = NewVehicle(odometer: 1000, createdDaysAgo: 10, fuelConsumption: 10);
        var ctx = new MaintenanceEvaluationContext(vehicle, BothIntervalsType(15000, 365), null, 14.0, 15000, 365);

        var result = _strategy.Evaluate(ctx);

        Assert.Contains("Spalanie powyżej normy", result.Message);
    }

    [Fact]
    public void Evaluate_DoesNotAddNote_WhenNormalUsage()
    {
        // FuelConsumption=10, threshold=12L, fuel consumed=8L → normal
        var vehicle = NewVehicle(odometer: 1000, createdDaysAgo: 10, fuelConsumption: 10);
        var ctx = new MaintenanceEvaluationContext(vehicle, BothIntervalsType(15000, 365), null, 8.0, 15000, 365);

        var result = _strategy.Evaluate(ctx);

        Assert.DoesNotContain("Spalanie powyżej normy", result.Message);
    }
}
