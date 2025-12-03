using System.Net;
using CarApi.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CarApi.Functions;

public class GetChargingSchedule
{
    private readonly ILogger<GetChargingSchedule> _logger;
    private readonly IDatabase _database;

    public GetChargingSchedule(ILogger<GetChargingSchedule> logger, IDatabase database)
    {
        _logger = logger;
        _database = database;
    }

    [Function("GetChargingSchedule")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "car/{deviceId}/schedules")] // For demo - change to AuthorizationLevel.Function for production
        HttpRequestData req,
        string deviceId)
    {
        _logger.LogInformation($"Getting charging schedules for device: {deviceId}");

        try
        {
            var schedules = _database.GetChargingSchedulesByDevice(deviceId);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(schedules);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting charging schedules");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = "Internal server error" });
            return errorResponse;
        }
    }
}
