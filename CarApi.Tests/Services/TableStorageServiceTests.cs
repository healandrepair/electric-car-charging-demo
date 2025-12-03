using Azure.Data.Tables;
using CarApi.Models;
using CarApi.Services;
using FluentAssertions;

namespace CarApi.Tests.Services;

public class TableStorageServiceTests
{
    private readonly TableStorageService _storage;

    public TableStorageServiceTests()
    {
        // Use Azurite local storage emulator connection string
        var connectionString = "UseDevelopmentStorage=true";

        try
        {
            _storage = new TableStorageService(connectionString);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Failed to connect to Azurite. Make sure Azurite is running or installed aye", ex);
        }
    }

    [Fact]
    public void SetCarStatus_Should_Store_CarStatus()
    {
        // Arrange
        var deviceId = Guid.NewGuid().ToString();
        var carStatus = new CarStatus
        {
            DeviceId = deviceId,
            BatteryLevel = 85.5,
            IsCharging = true,
            LastUpdated = DateTime.UtcNow
        };

        // Act
        _storage.SetCarStatus(carStatus);

        // Assert
        var result = _storage.GetCarStatus(deviceId);
        result.Should().NotBeNull();
        result!.DeviceId.Should().Be(deviceId);
        result.BatteryLevel.Should().Be(85.5);
        result.IsCharging.Should().BeTrue();
    }

    [Fact]
    public void GetCarStatus_Should_Return_Null_When_NotFound()
    {
        // Act
        var result = _storage.GetCarStatus(Guid.NewGuid().ToString());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void SetCarStatus_Should_Update_Existing_Status()
    {
        // Arrange
        var deviceId = Guid.NewGuid().ToString();
        var initialStatus = new CarStatus
        {
            DeviceId = deviceId,
            BatteryLevel = 40.0,
            IsCharging = false,
            LastUpdated = DateTime.UtcNow
        };

        var updatedStatus = new CarStatus
        {
            DeviceId = deviceId,
            BatteryLevel = 95.0,
            IsCharging = true,
            LastUpdated = DateTime.UtcNow.AddMinutes(5)
        };

        // Act
        _storage.SetCarStatus(initialStatus);
        _storage.SetCarStatus(updatedStatus);

        // Assert
        var result = _storage.GetCarStatus(deviceId);
        result.Should().NotBeNull();
        result!.BatteryLevel.Should().Be(95.0);
        result.IsCharging.Should().BeTrue();
    }

    [Fact]
    public void SetChargingSchedule_Should_Store_Schedule()
    {
        // Arrange
        var scheduleId = Guid.NewGuid().ToString();
        var deviceId = Guid.NewGuid().ToString();
        var schedule = new ChargingSchedule
        {
            Id = scheduleId,
            DeviceId = deviceId,
            ScheduledTime = DateTime.UtcNow.AddHours(3),
            IsCompleted = false
        };

        // Act
        _storage.SetChargingSchedule(schedule);

        // Assert
        var result = _storage.GetChargingSchedule(scheduleId);
        result.Should().NotBeNull();
        result!.Id.Should().Be(scheduleId);
        result.DeviceId.Should().Be(deviceId);
        result.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void GetChargingSchedule_Should_Return_Null_When_NotFound()
    {
        // Act
        var result = _storage.GetChargingSchedule(Guid.NewGuid().ToString());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetChargingSchedulesByDevice_Should_Return_Device_Schedules()
    {
        // Arrange
        var deviceId = Guid.NewGuid().ToString();
        var schedule1 = new ChargingSchedule
        {
            Id = Guid.NewGuid().ToString(),
            DeviceId = deviceId,
            ScheduledTime = DateTime.UtcNow.AddHours(2),
            IsCompleted = false
        };
        var schedule2 = new ChargingSchedule
        {
            Id = Guid.NewGuid().ToString(),
            DeviceId = deviceId,
            ScheduledTime = DateTime.UtcNow.AddHours(4),
            IsCompleted = false
        };
        var schedule3 = new ChargingSchedule
        {
            Id = Guid.NewGuid().ToString(),
            DeviceId = Guid.NewGuid().ToString(),
            ScheduledTime = DateTime.UtcNow.AddHours(6),
            IsCompleted = false
        };

        _storage.SetChargingSchedule(schedule1);
        _storage.SetChargingSchedule(schedule2);
        _storage.SetChargingSchedule(schedule3);

        // Act
        var results = _storage.GetChargingSchedulesByDevice(deviceId);

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(s => s.Id == schedule1.Id);
        results.Should().Contain(s => s.Id == schedule2.Id);
        results.Should().NotContain(s => s.Id == schedule3.Id);
    }

    [Fact]
    public void DeleteChargingSchedule_Should_Remove_Schedule()
    {
        // Arrange
        var scheduleId = Guid.NewGuid().ToString();
        var deviceId = Guid.NewGuid().ToString();
        var schedule = new ChargingSchedule
        {
            Id = scheduleId,
            DeviceId = deviceId,
            ScheduledTime = DateTime.UtcNow.AddHours(2),
            IsCompleted = false
        };

        _storage.SetChargingSchedule(schedule);

        // Act
        _storage.DeleteChargingSchedule(scheduleId);

        // Assert
        var result = _storage.GetChargingSchedule(scheduleId);
        result.Should().BeNull();
    }

    [Fact]
    public void DeleteChargingSchedule_Should_Not_Throw_When_NotFound()
    {
        // Act & Assert
        var act = () => _storage.DeleteChargingSchedule(Guid.NewGuid().ToString());
        act.Should().NotThrow();
    }

    [Fact]
    public void GetAllCarStatuses_Should_Return_All_Statuses()
    {
        // Arrange
        var deviceId1 = Guid.NewGuid().ToString();
        var deviceId2 = Guid.NewGuid().ToString();
        var status1 = new CarStatus { DeviceId = deviceId1, BatteryLevel = 60.0, IsCharging = true, LastUpdated = DateTime.UtcNow };
        var status2 = new CarStatus { DeviceId = deviceId2, BatteryLevel = 30.0, IsCharging = false, LastUpdated = DateTime.UtcNow };

        _storage.SetCarStatus(status1);
        _storage.SetCarStatus(status2);

        // Act
        var results = _storage.GetAllCarStatuses();

        // Assert
        results.Should().HaveCountGreaterThanOrEqualTo(2);
        results.Should().Contain(s => s.DeviceId == deviceId1);
        results.Should().Contain(s => s.DeviceId == deviceId2);
    }

    [Fact]
    public void GetAllChargingSchedules_Should_Return_All_Schedules()
    {
        // Arrange
        var schedule1 = new ChargingSchedule
        {
            Id = Guid.NewGuid().ToString(),
            DeviceId = Guid.NewGuid().ToString(),
            ScheduledTime = DateTime.UtcNow.AddHours(2),
            IsCompleted = false
        };
        var schedule2 = new ChargingSchedule
        {
            Id = Guid.NewGuid().ToString(),
            DeviceId = Guid.NewGuid().ToString(),
            ScheduledTime = DateTime.UtcNow.AddHours(3),
            IsCompleted = false
        };

        _storage.SetChargingSchedule(schedule1);
        _storage.SetChargingSchedule(schedule2);

        // Act
        var results = _storage.GetAllChargingSchedules();

        // Assert
        results.Should().HaveCountGreaterThanOrEqualTo(2);
        results.Should().Contain(s => s.Id == schedule1.Id);
        results.Should().Contain(s => s.Id == schedule2.Id);
    }

    [Fact]
    public void SetChargingSchedule_Should_Update_Existing_Schedule()
    {
        // Arrange
        var scheduleId = Guid.NewGuid().ToString();
        var deviceId = Guid.NewGuid().ToString();

        var initialSchedule = new ChargingSchedule
        {
            Id = scheduleId,
            DeviceId = deviceId,
            ScheduledTime = DateTime.UtcNow.AddHours(2),
            IsCompleted = false
        };

        var updatedSchedule = new ChargingSchedule
        {
            Id = scheduleId,
            DeviceId = deviceId,
            ScheduledTime = DateTime.UtcNow.AddHours(5),
            IsCompleted = true
        };

        // Act
        _storage.SetChargingSchedule(initialSchedule);
        _storage.SetChargingSchedule(updatedSchedule);

        // Assert
        var result = _storage.GetChargingSchedule(scheduleId);
        result.Should().NotBeNull();
        result!.IsCompleted.Should().BeTrue();
        result.ScheduledTime.Should().BeCloseTo(updatedSchedule.ScheduledTime, TimeSpan.FromSeconds(1));
    }
}
