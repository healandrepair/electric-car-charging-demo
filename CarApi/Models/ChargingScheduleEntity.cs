using Azure;
using Azure.Data.Tables;

namespace CarApi.Models;

public class ChargingScheduleEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty; // DeviceId
    public string RowKey { get; set; } = string.Empty; // Id
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Id { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public DateTime ScheduledTime { get; set; }
    public bool IsCompleted { get; set; }

    public ChargingScheduleEntity() { }

    public ChargingScheduleEntity(ChargingSchedule schedule)
    {
        PartitionKey = schedule.DeviceId;
        RowKey = schedule.Id;
        Id = schedule.Id;
        DeviceId = schedule.DeviceId;
        ScheduledTime = schedule.ScheduledTime;
        IsCompleted = schedule.IsCompleted;
    }

    public ChargingSchedule ToChargingSchedule()
    {
        return new ChargingSchedule
        {
            Id = string.IsNullOrEmpty(Id) ? RowKey : Id,
            DeviceId = string.IsNullOrEmpty(DeviceId) ? PartitionKey : DeviceId,
            ScheduledTime = ScheduledTime,
            IsCompleted = IsCompleted
        };
    }
}
