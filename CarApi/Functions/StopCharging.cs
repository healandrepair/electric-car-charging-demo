using System.Net;
using System.Text;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CarApi.Functions;

public class StopCharging
{
    private readonly ILogger<StopCharging> _logger;

    public StopCharging(ILogger<StopCharging> logger)
    {
        _logger = logger;
    }

    [Function("StopCharging")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "car/{deviceId}/charging/stop")] // For demo - change to AuthorizationLevel.Function for production
        HttpRequestData req,
        string deviceId)
    {
        _logger.LogInformation($"Stopping charging for device: {deviceId}");

        try
        {
            var iotHubConnectionString = Environment.GetEnvironmentVariable("IoTHubConnectionString");
            var serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);

            var commandMessage = new Message(Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(new { action = "stop" })));

            await serviceClient.SendAsync(deviceId, commandMessage);

            _logger.LogInformation($"Charging stop command sent to device: {deviceId}");

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { message = "Charging stopped", deviceId });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping charging");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = "Failed to stop charging" });
            return errorResponse;
        }
    }
}
