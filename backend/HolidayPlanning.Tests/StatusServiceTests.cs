using HolidayPlanning.Services;
using Xunit;

namespace HolidayPlanning.Tests;

/// <summary>
/// GET /api/status serialises exactly these values (version, friendlyStatus, timestamp),
/// so the service and the domain model's friendly text are what is asserted here.
/// </summary>
public class StatusServiceTests
{
    [Fact]
    public async Task Status_ReportsTheRunningVersion()
    {
        var status = await new StatusService().GetSystemStatusAsync();

        Assert.False(string.IsNullOrWhiteSpace(status.Version));
        Assert.Equal("1.1.0-alpha", status.Version);
    }

    [Fact]
    public async Task Status_FriendlyStatus_SaysWhichVersionIsRunning()
    {
        var status = await new StatusService().GetSystemStatusAsync();

        var friendly = status.GetFriendlyStatus();

        Assert.Contains("System is running version 1.1.0-alpha", friendly);
    }

    [Fact]
    public async Task Status_Timestamp_IsCurrentUtcTime()
    {
        var before = DateTime.UtcNow;

        var status = await new StatusService().GetSystemStatusAsync();

        Assert.InRange(status.LastUpdated, before, DateTime.UtcNow);
    }
}
