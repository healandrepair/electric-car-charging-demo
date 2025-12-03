using System.Net;
using System.Text;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CarApi.Functions;

public class StartCharging
{
    private readonly ILogger<StartCharging> _logger;

    public StartCharging(ILogger<StartCharging> logger)
    {
        _logger = logger;
    }

    [Function("StartCharging")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "car/{deviceId}/charging/start")] // For demo - change to AuthorizationLevel.Function for production
        HttpRequestData req,
        string deviceId)
    {
        _logger.LogInformation($"Starting charging for device: {deviceId}");

        try
        {
            var iotHubConnectionString = Environment.GetEnvironmentVariable("IoTHubConnectionString");
            var serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);

            var commandMessage = new Message(Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(new { action = "start" })));

            await serviceClient.SendAsync(deviceId, commandMessage);

            _logger.LogInformation($"Charging start command sent to device: {deviceId}");

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { message = "Charging started", deviceId });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting charging");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = "Failed to start charging" });
            return errorResponse;
        }
    }
}
