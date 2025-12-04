export interface BatteryStatus {
  batteryLevel: number;
  isCharging: boolean;
  lastUpdated: Date;
  deviceId: string;
}

export interface ChargingCommand {
  deviceId: string;
  action: 'start' | 'stop';
}

export interface ChargingSchedule {
  id?: string;
  deviceId: string;
  scheduledTime: Date;
  isCompleted: boolean;
}
