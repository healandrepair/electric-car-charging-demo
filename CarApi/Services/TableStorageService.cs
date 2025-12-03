using Azure.Data.Tables;
using CarApi.Interfaces;
using CarApi.Models;

namespace CarApi.Services;

public class TableStorageService : IDatabase
{
    private readonly TableClient _carStatusTable;
    private readonly TableClient _chargingScheduleTable;

    public TableStorageService(string connectionString)
    {
        var tableServiceClient = new TableServiceClient(connectionString);

        _carStatusTable = tableServiceClient.GetTableClient("CarStatus");
        _chargingScheduleTable = tableServiceClient.GetTableClient("ChargingSchedule");

        // Create tables if they don't exist
        _carStatusTable.CreateIfNotExists();
        _chargingScheduleTable.CreateIfNotExists();
    }

    public CarStatus? GetCarStatus(string deviceId)
    {
        try
        {
            var entity = _carStatusTable.GetEntity<CarStatusEntity>("CarStatus", deviceId);
            return entity.Value.ToCarStatus();
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public void SetCarStatus(CarStatus status)
    {
        var entity = new CarStatusEntity(status);
        _carStatusTable.UpsertEntity(entity);
    }

    public ChargingSchedule? GetChargingSchedule(string scheduleId)
    {
        try
        {
            // Need to query across all partitions to find by schedule ID
            var entities = _chargingScheduleTable.Query<ChargingScheduleEntity>(
                filter: $"RowKey eq '{scheduleId}'");
            return entities.FirstOrDefault()?.ToChargingSchedule();
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public IList<ChargingSchedule> GetChargingSchedulesByDevice(string deviceId)
    {
        var schedules = new List<ChargingSchedule>();
        var entities = _chargingScheduleTable.Query<ChargingScheduleEntity>(
            filter: $"PartitionKey eq '{deviceId}'");

        foreach (var entity in entities)
        {
            schedules.Add(entity.ToChargingSchedule());
        }

        return schedules;
    }

    public void SetChargingSchedule(ChargingSchedule schedule)
    {
        var entity = new ChargingScheduleEntity(schedule);
        _chargingScheduleTable.UpsertEntity(entity);
    }

    public void DeleteChargingSchedule(string scheduleId)
    {
        try
        {
            // Need to find the entity first to get the partition key
            var entities = _chargingScheduleTable.Query<ChargingScheduleEntity>(
                filter: $"RowKey eq '{scheduleId}'");
            var entity = entities.FirstOrDefault();

            if (entity != null)
            {
                _chargingScheduleTable.DeleteEntity(entity.PartitionKey, entity.RowKey);
            }
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            // Schedule not found, ignore
        }
    }

    public IList<ChargingSchedule> GetAllChargingSchedules()
    {
        var schedules = new List<ChargingSchedule>();
        var entities = _chargingScheduleTable.Query<ChargingScheduleEntity>();

        foreach (var entity in entities)
        {
            schedules.Add(entity.ToChargingSchedule());
        }

        return schedules;
    }

    public IList<CarStatus> GetAllCarStatuses()
    {
        var statuses = new List<CarStatus>();
        var entities = _carStatusTable.Query<CarStatusEntity>();

        foreach (var entity in entities)
        {
            statuses.Add(entity.ToCarStatus());
        }

        return statuses;
    }
}
