import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, interval } from 'rxjs';
import { switchMap, startWith, map } from 'rxjs/operators';
import { BatteryStatus, ChargingCommand, ChargingSchedule } from '../models/battery.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CarBatteryService {
  private apiUrl = environment.apiUrl;

  private readonly NZ_TIMEZONE = 'Pacific/Auckland';

  constructor(private http: HttpClient) { }

  getBatteryStatus(deviceId: string): Observable<BatteryStatus> {
    return this.http.get<any>(`${this.apiUrl}/car/${deviceId}/status`).pipe(
      map(status => ({
        batteryLevel: status.BatteryLevel || status.batteryLevel || 0,
        isCharging: status.IsCharging || status.isCharging || false,
        deviceId: status.DeviceId || status.deviceId || 'unknown',
        lastUpdated: status.LastUpdated || status.lastUpdated ? new Date(status.LastUpdated || status.lastUpdated) : new Date()
      }))
    );
  }

  // Poll for battery status every 5 seconds
  getBatteryStatusPolling(deviceId: string): Observable<BatteryStatus> {
    return interval(5000).pipe(
      startWith(0),
      switchMap(() => this.getBatteryStatus(deviceId))
    );
  }

  startCharging(deviceId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/car/${deviceId}/charging/start`, {});
  }

  stopCharging(deviceId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/car/${deviceId}/charging/stop`, {});
  }

  setChargingSchedule(schedule: ChargingSchedule): Observable<ChargingSchedule> {
    // API expects UTC DateTime, so send the Date object as-is (browser will serialize to ISO string)
    return this.http.post<any>(`${this.apiUrl}/car/${schedule.deviceId}/schedules`, schedule).pipe(
      map(response => ({
        id: response.Id || response.id,
        deviceId: response.DeviceId || response.deviceId,
        scheduledTime: new Date(response.ScheduledTime || response.scheduledTime),
        isCompleted: response.IsCompleted ?? response.isCompleted ?? false
      }))
    );
  }

  getChargingSchedules(deviceId: string): Observable<ChargingSchedule[]> {
    return this.http.get<any[]>(`${this.apiUrl}/car/${deviceId}/schedules`).pipe(
      map(schedules => schedules.map(schedule => ({
        id: schedule.Id || schedule.id,
        deviceId: schedule.DeviceId || schedule.deviceId,
        scheduledTime: new Date(schedule.ScheduledTime || schedule.scheduledTime),
        isCompleted: schedule.IsCompleted ?? schedule.isCompleted ?? false
      })))
    );
  }

  deleteChargingSchedule(deviceId: string, scheduleId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/car/${deviceId}/schedules/${scheduleId}`);
  }

  // Helper: Convert NZ time string (HH:mm) to UTC Date for today
  convertNZTimeToUTC(timeString: string): Date {
    const [hours, minutes] = timeString.split(':').map(Number);

    // Create date in NZ timezone for today
    const nzDate = new Date();
    nzDate.setHours(hours, minutes, 0, 0);

    // Get NZ offset (NZST = UTC+12, NZDT = UTC+13)
    // This automatically handles daylight saving
    const nzOffset = this.getNZOffset();

    // Convert to UTC by subtracting NZ offset
    const utcDate = new Date(nzDate.getTime() - (nzOffset * 60 * 60 * 1000));

    return utcDate;
  }

  // Helper: Convert NZ date and time to UTC Date
  convertNZDateTimeToUTC(dateString: string, timeString: string): Date {
    const [hours, minutes] = timeString.split(':').map(Number);
    const [year, month, day] = dateString.split('-').map(Number);

    console.log('convertNZDateTimeToUTC - dateString:', dateString, 'timeString:', timeString);
    console.log('Parsed year:', year, 'month:', month, 'day:', day, 'hours:', hours, 'minutes:', minutes);

    // Create a date in UTC representing what we THINK is the NZ time
    // We'll create it as UTC, then adjust
    const utcDate = new Date(Date.UTC(year, month - 1, day, hours, minutes, 0, 0));
    console.log('Date as if it were UTC:', utcDate);

    // Get NZ offset (NZST = UTC+12, NZDT = UTC+13)
    const nzOffset = this.getNZOffset();
    console.log('NZ offset:', nzOffset);

    // Since we created the date as if it were UTC, but it's actually NZ time,
    // we need to subtract the offset to get the actual UTC time
    const actualUtcDate = new Date(utcDate.getTime() - (nzOffset * 60 * 60 * 1000));
    console.log('Final UTC date:', actualUtcDate);

    return actualUtcDate;
  }

  // Helper: Convert UTC Date to NZ time string (HH:mm)
  convertUTCToNZTime(utcDate: Date): string {
    const nzOffset = this.getNZOffset();
    const nzDate = new Date(utcDate.getTime() + (nzOffset * 60 * 60 * 1000));

    const hours = nzDate.getHours().toString().padStart(2, '0');
    const minutes = nzDate.getMinutes().toString().padStart(2, '0');

    return `${hours}:${minutes}`;
  }

  // Helper: Convert UTC Date to local browser date and time string
  convertUTCToNZDateTime(utcDate: Date): string {
    // Use browser's local timezone for display
    const year = utcDate.getFullYear();
    const month = (utcDate.getMonth() + 1).toString().padStart(2, '0');
    const day = utcDate.getDate().toString().padStart(2, '0');
    const hours = utcDate.getHours().toString().padStart(2, '0');
    const minutes = utcDate.getMinutes().toString().padStart(2, '0');

    return `${day}/${month}/${year} ${hours}:${minutes}`;
  }

  // Get NZ timezone offset in hours (handles DST automatically)
  private getNZOffset(): number {
    // Check if we're in daylight saving (NZDT = UTC+13, NZST = UTC+12)
    const now = new Date();
    const jan = new Date(now.getFullYear(), 0, 1);
    const jul = new Date(now.getFullYear(), 6, 1);
    const stdOffset = Math.max(jan.getTimezoneOffset(), jul.getTimezoneOffset());
    const isDST = now.getTimezoneOffset() < stdOffset;

    // NZ is UTC+12 (NZST) or UTC+13 (NZDT)
    return isDST ? 13 : 12;
  }
}
