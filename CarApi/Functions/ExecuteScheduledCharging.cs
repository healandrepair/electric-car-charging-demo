using System.Text;
using CarApi.Interfaces;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CarApi.Functions;

public class ExecuteScheduledCharging
{
    private readonly ILogger<ExecuteScheduledCharging> _logger;
    private readonly IDatabase _database;

    public ExecuteScheduledCharging(ILogger<ExecuteScheduledCharging> logger, IDatabase database)
    {
        _logger = logger;
        _database = database;
    }

    [Function("ExecuteScheduledCharging")]
    public async Task Run([TimerTrigger("0 * * * * *")] TimerInfo timerInfo)
    {
        _logger.LogInformation($"Executing scheduled charging check at: {DateTime.UtcNow}");

        try
        {
            var currentTime = DateTime.UtcNow;
            var schedules = _database.GetAllChargingSchedules();

            foreach (var schedule in schedules)
            {
                if (schedule.IsCompleted)
                    continue;

                // Check if scheduled time has passed (ScheduledTime is stored in UTC)
                if (schedule.ScheduledTime <= currentTime)
                {
                    _logger.LogInformation($"Triggering scheduled charging for device: {schedule.DeviceId} (scheduled: {schedule.ScheduledTime}, current: {currentTime})");

                    var iotHubConnectionString = Environment.GetEnvironmentVariable("IoTHubConnectionString");
                    var serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);

                    var commandMessage = new Message(Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(new { action = "start" })));

                    await serviceClient.SendAsync(schedule.DeviceId, commandMessage);

                    _logger.LogInformation($"Scheduled charging command sent to device: {schedule.DeviceId}");

                    // set schedule as completed
                    schedule.IsCompleted = true;
                    _database.SetChargingSchedule(schedule);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing scheduled charging");
        }
    }
}
