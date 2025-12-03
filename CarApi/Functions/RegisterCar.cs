using System.Net;
using CarApi.Interfaces;
using CarApi.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CarApi.Functions;

public class RegisterCar
{
    private readonly ILogger<RegisterCar> _logger;
    private readonly IDatabase _database;

    public RegisterCar(ILogger<RegisterCar> logger, IDatabase database)
    {
        _logger = logger;
        _database = database;
    }

    [Function("RegisterCar")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "car/{deviceId}/register")]
        HttpRequestData req,
        string deviceId)
    {
        _logger.LogInformation($"Registering new car: {deviceId}");

        try
        {
            // Check if car already exists
            var existingCar = _database.GetCarStatus(deviceId);
            if (existingCar != null)
            {
                var conflictResponse = req.CreateResponse(HttpStatusCode.Conflict);
                await conflictResponse.WriteAsJsonAsync(new { error = "Car already registered" });
                return conflictResponse;
            }

            var newCar = new CarStatus
            {
                DeviceId = deviceId,
                BatteryLevel = 50.0,
                IsCharging = false,
                LastUpdated = DateTime.UtcNow
            };

            _database.SetCarStatus(newCar);

            _logger.LogInformation($"Car registered successfully: {deviceId}");

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(new
            {
                message = "Car registered successfully",
                deviceId = deviceId,
                status = newCar
            });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering car");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = "Failed to register car" });
            return errorResponse;
        }
    }
}
