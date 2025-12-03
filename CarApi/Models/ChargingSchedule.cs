namespace CarApi.Models;

public class ChargingSchedule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DeviceId { get; set; } = string.Empty;
    public DateTime ScheduledTime { get; set; }
    public bool IsCompleted { get; set; } = false;
}
