# Car Battery Management API

Azure Functions REST API for managing electric car battery charging via IoT Hub.

## Features

- Real-time battery telemetry from IoT Hub
- Start/stop charging commands
- Schedule charging sessions
- Automatic scheduled execution
- Status monitoring

## Tech Stack

- IoT Hub - device telemetry
- Azure Functions - API + message processing
- Table Storage - persistent data
- Cloud-to-Device messaging - charging commands

## API Endpoints

### GET `/api/car/{deviceId}/status`
Returns current battery status

### POST `/api/car/{deviceId}/charging/start`
Start charging

### POST `/api/car/{deviceId}/charging/stop`
Stop charging

### POST `/api/car/{deviceId}/schedules`
Create charging schedule
```json
{
  "scheduledTime": "2025-12-04T02:00:00Z",
  "isCompleted": false
}
```

### GET `/api/car/{deviceId}/schedules`
List all schedules for device

### DELETE `/api/car/{deviceId}/schedules/{scheduleId}`
Delete a schedule

### ExecuteScheduledCharging (Timer)
Runs every minute, triggers scheduled charging

## Setup

### Azure Resources

```bash
az group create --name car-battery-rg --location eastus

az storage account create --name carbatterystorage \
  --resource-group car-battery-rg --location eastus --sku Standard_LRS

az iot hub create --name car-battery-hub \
  --resource-group car-battery-rg --sku F1 --partition-count 2

az iot hub device-identity create --hub-name car-battery-hub --device-id my-car
```

### Connection Strings

```bash
az storage account show-connection-string --name carbatterystorage --resource-group car-battery-rg
az iot hub connection-string show --hub-name car-battery-hub --policy-name service
az iot hub connection-string show --hub-name car-battery-hub --default-eventhub
az iot hub device-identity connection-string show --hub-name car-battery-hub --device-id my-car
```

### local.settings.json

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "<storage-connection-string>",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "IoTHubConnectionString": "<iothub-service-connection-string>",
    "IoTHubEventHubEndpoint": "<iothub-eventhub-endpoint>"
  }
}
```

### Run Locally

```bash
dotnet build
func start
```

### Deploy

```bash
az functionapp create --resource-group car-battery-rg \
  --consumption-plan-location eastus --runtime dotnet-isolated \
  --functions-version 4 --name car-battery-api --storage-account carbatterystorage

func azure functionapp publish car-battery-api

az functionapp config appsettings set --name car-battery-api \
  --resource-group car-battery-rg \
  --settings "IoTHubConnectionString=<value>" "IoTHubEventHubEndpoint=<value>"
```

## Testing

```bash
curl http://localhost:7071/api/car/my-car/status
curl -X POST http://localhost:7071/api/car/my-car/charging/start
curl -X POST http://localhost:7071/api/car/my-car/charging/stop
```

## CORS

```bash
az functionapp cors add --name car-battery-api --resource-group car-battery-rg \
  --allowed-origins "http://localhost:4200"
```

## Tests

Requires Azurite running:
```bash
npm install -g azurite
azurite &
dotnet test
```

11 integration tests for TableStorageService.


## Project Structure

```
CarApi/
├── Functions/          # HTTP and timer triggers
├── Models/             # Data models
├── Services/           # Storage layer
│   ├── InMemoryStorage.cs
│   └── TableStorageService.cs
├── Interfaces/         # IDatabase interface
└── Program.cs
```

## Notes

- Data persists in Table Storage across restarts
- Tables auto-created: `CarStatus`, `ChargingSchedule`
