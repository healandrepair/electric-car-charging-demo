using System.Net;
using CarApi.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CarApi.Functions;

public class ListCars
{
    private readonly ILogger<ListCars> _logger;
    private readonly IDatabase _database;

    public ListCars(ILogger<ListCars> logger, IDatabase database)
    {
        _logger = logger;
        _database = database;
    }

    [Function("ListCars")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "cars")]
        HttpRequestData req)
    {
        _logger.LogInformation("Getting list of all cars");

        try
        {
            var cars = _database.GetAllCarStatuses();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                count = cars.Count,
                cars = cars
            });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing cars");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = "Failed to list cars" });
            return errorResponse;
        }
    }
}
