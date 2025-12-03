namespace CarApi.Models;

public class CarTelemetry
{
    public string DeviceId { get; set; } = string.Empty;
    public double BatteryLevel { get; set; }
    public bool IsCharging { get; set; }
    public DateTime Timestamp { get; set; }
}
