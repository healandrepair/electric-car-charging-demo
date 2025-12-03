using System.Net;
using CarApi.Interfaces;
using CarApi.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CarApi.Functions;

public class SetChargingSchedule
{
    private readonly ILogger<SetChargingSchedule> _logger;
    private readonly IDatabase _database;

    public SetChargingSchedule(ILogger<SetChargingSchedule> logger, IDatabase database)
    {
        _logger = logger;
        _database = database;
    }

    [Function("SetChargingSchedule")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "car/{deviceId}/schedules")] // For demo - change to AuthorizationLevel.Function for production
        HttpRequestData req,
        string deviceId)
    {
        _logger.LogInformation($"Setting charging schedule for device: {deviceId}");

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var schedule = JsonConvert.DeserializeObject<ChargingSchedule>(requestBody);

            if (schedule == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteAsJsonAsync(new { error = "Invalid schedule data" });
                return badRequest;
            }

            schedule.DeviceId = deviceId;
            if (string.IsNullOrEmpty(schedule.Id))
            {
                schedule.Id = Guid.NewGuid().ToString();
            }

            // Validate that the scheduled time is in the future
            if (schedule.ScheduledTime <= DateTime.UtcNow)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteAsJsonAsync(new { error = "Cannot schedule charging in the past. Please select a future time." });
                return badRequest;
            }
            
            schedule.IsCompleted = false;
            _database.SetChargingSchedule(schedule);

            _logger.LogInformation($"Schedule {schedule.Id} set for device: {deviceId}");

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(schedule);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting charging schedule");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = "Failed to set schedule" });
            return errorResponse;
        }
    }
}
