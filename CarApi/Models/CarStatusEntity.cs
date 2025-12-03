using Azure;
using Azure.Data.Tables;

namespace CarApi.Models;

public class CarStatusEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "CarStatus";
    public string RowKey { get; set; } = string.Empty; // DeviceId
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string DeviceId { get; set; } = string.Empty;
    public double BatteryLevel { get; set; }
    public bool IsCharging { get; set; }
    public DateTime LastUpdated { get; set; }

    public CarStatusEntity() { }

    public CarStatusEntity(CarStatus status)
    {
        RowKey = status.DeviceId;
        DeviceId = status.DeviceId;
        BatteryLevel = status.BatteryLevel;
        IsCharging = status.IsCharging;
        LastUpdated = status.LastUpdated;
    }

    public CarStatus ToCarStatus()
    {
        return new CarStatus
        {
            DeviceId = DeviceId,
            BatteryLevel = BatteryLevel,
            IsCharging = IsCharging,
            LastUpdated = LastUpdated
        };
    }
}
