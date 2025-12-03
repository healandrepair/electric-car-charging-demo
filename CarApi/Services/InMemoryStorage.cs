using System.Collections.Concurrent;
using CarApi.Interfaces;
using CarApi.Models;

namespace CarApi.Services;

// Unused, inMemory implementation was first idea but favored out due to cons.
public class InMemoryStorage : IDatabase
{
    private static readonly ConcurrentDictionary<string, CarStatus> _carStatuses = new();
    private static readonly ConcurrentDictionary<string, ChargingSchedule> _chargingSchedules = new();

    public CarStatus? GetCarStatus(string deviceId)
    {
        _carStatuses.TryGetValue(deviceId, out var status);
        return status;
    }

    public void SetCarStatus(CarStatus status)
    {
        _carStatuses[status.DeviceId] = status;
    }

    public ChargingSchedule? GetChargingSchedule(string scheduleId)
    {
        _chargingSchedules.TryGetValue(scheduleId, out var schedule);
        return schedule;
    }

    public IList<ChargingSchedule> GetChargingSchedulesByDevice(string deviceId)
    {
        return _chargingSchedules.Values
            .Where(s => s.DeviceId == deviceId)
            .ToList();
    }

    public void SetChargingSchedule(ChargingSchedule schedule)
    {
        _chargingSchedules[schedule.Id] = schedule;
    }

    public void DeleteChargingSchedule(string scheduleId)
    {
        _chargingSchedules.TryRemove(scheduleId, out _);
    }

    public IList<ChargingSchedule> GetAllChargingSchedules()
    {
        return _chargingSchedules.Values.ToList();
    }

    public IList<CarStatus> GetAllCarStatuses()
    {
        return _carStatuses.Values.ToList();
    }
}
