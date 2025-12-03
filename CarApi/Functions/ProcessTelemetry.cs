using CarApi.Interfaces;
using CarApi.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CarApi.Functions;

public class ProcessTelemetry
{
    private readonly ILogger<ProcessTelemetry> _logger;
    private readonly IDatabase _database;

    public ProcessTelemetry(ILogger<ProcessTelemetry> logger, IDatabase database)
    {
        _logger = logger;
        _database = database;
    }

    [Function("ProcessTelemetry")]
    public Task Run(
        [EventHubTrigger("messages/events", Connection = "IoTHubEventHubEndpoint")] string[] events)
    {
        foreach (var eventData in events)
        {
            try
            {
                _logger.LogInformation($"Processing telemetry: {eventData}");

                var telemetry = JsonConvert.DeserializeObject<CarTelemetry>(eventData);

                if (telemetry == null)
                {
                    _logger.LogWarning("Failed to deserialize telemetry data");
                    continue;
                }

                var carStatus = new CarStatus
                {
                    DeviceId = telemetry.DeviceId,
                    BatteryLevel = telemetry.BatteryLevel,
                    IsCharging = telemetry.IsCharging,
                    LastUpdated = telemetry.Timestamp
                };

                _database.SetCarStatus(carStatus);

                _logger.LogInformation($"Updated status for device: {telemetry.DeviceId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing telemetry");
            }
        }

        return Task.CompletedTask;
    }
}
