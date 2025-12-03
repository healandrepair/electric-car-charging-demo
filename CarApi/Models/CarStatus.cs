namespace CarApi.Models;

public class CarStatus
{
    public string DeviceId { get; set; } = string.Empty;
    public double BatteryLevel { get; set; }
    public bool IsCharging { get; set; }
    public DateTime LastUpdated { get; set; }
}
