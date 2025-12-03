using CarApi.Interfaces;
using CarApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration["AzureWebJobsStorage"];
        services.AddSingleton<IDatabase>(sp => new TableStorageService(connectionString!));
    })
    .Build();

host.Run();
