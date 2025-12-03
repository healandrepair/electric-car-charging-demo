using System.Net;
using CarApi.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CarApi.Functions;

public class GetCarStatus
{
    private readonly ILogger<GetCarStatus> _logger;
    private readonly IDatabase _database;

    public GetCarStatus(ILogger<GetCarStatus> logger, IDatabase database)
    {
        _logger = logger;
        _database = database;
    }

    [Function("GetCarStatus")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous,  // For demo - change to AuthorizationLevel.Function for production
            "get",
            Route = "car/{deviceId}/status")]
            HttpRequestData req,
            string deviceId)
    {
        _logger.LogInformation($"Getting status for device: {deviceId}");

        try
        {
            var carStatus = _database.GetCarStatus(deviceId);

            if (carStatus == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteAsJsonAsync(new { error = "Car not found" });
                return notFoundResponse;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(carStatus);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting car status");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = "Internal server error" });
            return errorResponse;
        }
    }
}
