using System.Net;
using CarApi.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CarApi.Functions;

public class DeleteChargingSchedule
{
    private readonly ILogger<DeleteChargingSchedule> _logger;
    private readonly IDatabase _database;

    public DeleteChargingSchedule(ILogger<DeleteChargingSchedule> logger, IDatabase database)
    {
        _logger = logger;
        _database = database;
    }

    [Function("DeleteChargingSchedule")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "car/{deviceId}/schedules/{scheduleId}")] // For demo - change to AuthorizationLevel.Function for production
        HttpRequestData req,
        string deviceId,
        string scheduleId)
    {
        _logger.LogInformation($"Deleting charging schedule {scheduleId} for device: {deviceId}");

        try
        {
            // Verify the schedule exists and belongs to the device
            var schedule = _database.GetChargingSchedule(scheduleId);

            if (schedule == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteAsJsonAsync(new { error = "Schedule not found" });
                return notFoundResponse;
            }

            if (schedule.DeviceId != deviceId)
            {
                var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteAsJsonAsync(new { error = "Schedule does not belong to this device" });
                return forbiddenResponse;
            }

            _database.DeleteChargingSchedule(scheduleId);

            _logger.LogInformation($"Schedule {scheduleId} deleted for device: {deviceId}");

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { message = "Schedule deleted successfully" });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting charging schedule");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = "Failed to delete schedule" });
            return errorResponse;
        }
    }
}
