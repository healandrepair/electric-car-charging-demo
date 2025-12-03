using CarApi.Models;

namespace CarApi.Interfaces;

public interface IDatabase
{
    public CarStatus? GetCarStatus(string deviceId);

    public void SetCarStatus(CarStatus status);

    public ChargingSchedule? GetChargingSchedule(string scheduleId);

    public IList<ChargingSchedule> GetChargingSchedulesByDevice(string deviceId);

    public void SetChargingSchedule(ChargingSchedule schedule);

    public void DeleteChargingSchedule(string scheduleId);

    public IList<ChargingSchedule> GetAllChargingSchedules();

    public IList<CarStatus> GetAllCarStatuses();
}