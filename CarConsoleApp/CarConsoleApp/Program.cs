using System.Text;
using System.Text.Json;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using CarConsoleApp.Constants;

// Temp Hardcode for sake of easy use, not for use in production. DONOTCOMMIT SECRETS
string connectionString = "";

DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(connectionString);

// Read initial state from device twin
var twinDevice = await deviceClient.GetTwinAsync();
var batteryLevel = InitaliseBatteryLevel(twinDevice);
var isCharging = IntialiseIsCharging(twinDevice);

// Start listening for commands in background
Task.Run(() => ReceiveCommandsAsync(deviceClient));

while (true)
{
    var telemetry = new
    {
        deviceId = "my-car", // TEST ONLY, Update deviceId to be not hardcoded for production use.
        batteryLevel,
        isCharging,
        timestamp = DateTime.UtcNow
    };
    var messageString = JsonSerializer.Serialize(telemetry);
    var message = new Message(Encoding.UTF8.GetBytes(messageString));

    await deviceClient.SendEventAsync(message);
    if (isCharging)
    {
        Console.WriteLine($"{telemetry.deviceId}: Battery={batteryLevel}%, Currently charging.");
    }
    else
    {
        Console.WriteLine($"{telemetry.deviceId}: Battery={batteryLevel}%, Currently not charging.");
    }

    // Simulate battery charging/draining
    int previousBatteryLevel = batteryLevel;
    if (isCharging && batteryLevel < 100)
    {
        batteryLevel += 3;
        if (batteryLevel > 100)
            batteryLevel = 100;
    }
    else if (!isCharging && batteryLevel > 0)
    {
        batteryLevel -= 1;
        if (batteryLevel < 0)
            batteryLevel = 0;
    }

    // Update device twin if battery level changed
    if (previousBatteryLevel != batteryLevel)
    {
        var reportedProperties = new
        {
            batteryLevel,
            isCharging
        };
        var reportedPropertiesJson = JsonSerializer.Serialize(reportedProperties);
        var patch = new TwinCollection(reportedPropertiesJson);
        await deviceClient.UpdateReportedPropertiesAsync(patch);
    }

    await Task.Delay(5000);
}

// Method to receive commands from cloud
async Task ReceiveCommandsAsync(DeviceClient deviceClient)
{
    Console.WriteLine("Listening for commands...\n");

    while (true)
    {
        Message receivedMessage = await deviceClient.ReceiveAsync();

        if (receivedMessage != null)
        {
            string messageData = Encoding.UTF8.GetString(receivedMessage.GetBytes());
            Console.WriteLine($"Recieved: {messageData}");

            // Process the command
            if (messageData.Contains(CarConstants.Start, StringComparison.OrdinalIgnoreCase))
            {
                isCharging = true;
                Console.WriteLine("Started charging");

                // Update device twin
                var reportedProperties = new { isCharging };
                var reportedPropertiesJson = JsonSerializer.Serialize(reportedProperties);
                var patch = new TwinCollection(reportedPropertiesJson);
                await deviceClient.UpdateReportedPropertiesAsync(patch);
            }
            else if (messageData.Contains(CarConstants.Stop, StringComparison.OrdinalIgnoreCase))
            {
                isCharging = false;
                Console.WriteLine("Stopped charging");

                // Update device twin
                var reportedProperties = new { isCharging };
                var reportedPropertiesJson = JsonSerializer.Serialize(reportedProperties);
                var patch = new TwinCollection(reportedPropertiesJson);
                await deviceClient.UpdateReportedPropertiesAsync(patch);
            }

            // Tell IoT Hub we processed the message
            await deviceClient.CompleteAsync(receivedMessage);
        }

        await Task.Delay(1000);
    }
}

bool IntialiseIsCharging(Twin twin)
{
    if (!twin.Properties.Reported.Contains(CarConstants.IsCharging))
    {
        return false;
    }

    bool initIsCharging = twin.Properties.Reported[CarConstants.IsCharging];
    Console.WriteLine($"Loaded charging state from device twin: {initIsCharging}");
    return initIsCharging;
}

int InitaliseBatteryLevel(Twin twin)
{
    var initBatteryLevel = 50;
    if (twin.Properties.Reported.Contains(CarConstants.BatteryLevel))
    {
        initBatteryLevel = twin.Properties.Reported[CarConstants.BatteryLevel];
        Console.WriteLine($"Loaded battery level from device twin: {initBatteryLevel}%");
    }
    else
    {
        Console.WriteLine("No previous battery level found, starting at 50%");
    }

    return initBatteryLevel;
}