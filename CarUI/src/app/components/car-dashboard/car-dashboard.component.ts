import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { CarBatteryService } from '../../services/car-battery.service';
import { BatteryStatus, ChargingSchedule } from '../../models/battery.model';

@Component({
  selector: 'app-car-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './car-dashboard.component.html',
  styleUrl: './car-dashboard.component.css'
})
export class CarDashboardComponent implements OnInit, OnDestroy {
  deviceId: string = 'my-car'; // Default device ID
  batteryStatus: BatteryStatus | null = null;
  chargingSchedules: ChargingSchedule[] = [];

  // For creating new schedule
  newScheduleDate: string = '';
  newScheduleTime: string = '06:00';

  isLoading: boolean = false;
  isChargingActionInProgress: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';

  // Validation popup state
  showValidationPopup: boolean = false;
  validationMessage: string = '';

  private statusSubscription?: Subscription;

  constructor(private carBatteryService: CarBatteryService) { }

  ngOnInit(): void {
    this.loadBatteryStatus();
    this.loadChargingSchedules();
    // Set default date to today
    const today = new Date();
    this.newScheduleDate = today.toISOString().split('T')[0];
  }

  ngOnDestroy(): void {
    if (this.statusSubscription) {
      this.statusSubscription.unsubscribe();
    }
  }

  loadBatteryStatus(): void {
    this.isLoading = true;
    this.errorMessage = '';

    // Subscribe to polling for real-time updates
    this.statusSubscription = this.carBatteryService.getBatteryStatusPolling(this.deviceId)
      .subscribe({
        next: (status) => {
          this.batteryStatus = status;
          this.isLoading = false;
          // Re-enable charging buttons when status updates
          this.isChargingActionInProgress = false;
        },
        error: (error) => {
          this.errorMessage = 'Failed to load battery status. Make sure the backend is running.';
          this.isLoading = false;
          console.error('Error loading battery status:', error);
        }
      });
  }

  loadChargingSchedules(): void {
    this.carBatteryService.getChargingSchedules(this.deviceId)
      .subscribe({
        next: (schedules) => {
          console.log('Loaded schedules:', schedules); // Debug log
          this.chargingSchedules = schedules;
        },
        error: (error) => {
          console.error('Error loading charging schedules:', error);
        }
      });
  }

  startCharging(): void {
    this.isChargingActionInProgress = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.carBatteryService.startCharging(this.deviceId)
      .subscribe({
        next: () => {
          // Button will re-enable automatically when status updates via polling
        },
        error: (error) => {
          this.isChargingActionInProgress = false;
          this.errorMessage = 'Failed to start charging. Please try again.';
          console.error('Error starting charging:', error);
        }
      });
  }

  stopCharging(): void {
    this.isChargingActionInProgress = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.carBatteryService.stopCharging(this.deviceId)
      .subscribe({
        next: () => {
          // Button will re-enable automatically when status updates via polling
        },
        error: (error) => {
          this.isChargingActionInProgress = false;
          this.errorMessage = 'Failed to stop charging. Please try again.';
          console.error('Error stopping charging:', error);
        }
      });
  }

  addSchedule(): void {
    this.errorMessage = '';
    this.successMessage = '';

    // Validate that date and time are provided
    if (!this.newScheduleDate || !this.newScheduleTime) {
      this.showValidationPopup = true;
      this.validationMessage = 'Please select both date and time for the schedule.';
      return;
    }

    // Combine date and time, then convert NZ time to UTC
    const utcTime = this.carBatteryService.convertNZDateTimeToUTC(this.newScheduleDate, this.newScheduleTime);
    const currentTime = new Date();

    // Validate that the scheduled time is in the future
    if (utcTime <= new Date()) {
      this.showValidationPopup = true;
      this.validationMessage = 'Cannot schedule charging in the past. Please select a future date and time.';
      return;
    }

    this.isLoading = true;

    const newSchedule: ChargingSchedule = {
      deviceId: this.deviceId,
      scheduledTime: utcTime,
      isCompleted: false
    };

    this.carBatteryService.setChargingSchedule(newSchedule)
      .subscribe({
        next: () => {
          this.successMessage = 'Charging schedule added successfully';
          this.isLoading = false;
          this.loadChargingSchedules();
          setTimeout(() => this.successMessage = '', 3000);
        },
        error: (error) => {
          // Check if it's a validation error from the API
          if (error.status === 400 && error.error?.error) {
            this.showValidationPopup = true;
            this.validationMessage = error.error.error;
          } else {
            this.errorMessage = 'Failed to add charging schedule';
          }
          this.isLoading = false;
          console.error('Error adding schedule:', error);
        }
      });
  }

  closeValidationPopup(): void {
    this.showValidationPopup = false;
  }

  deleteSchedule(scheduleId: string): void {
    if (!confirm('Are you sure you want to delete this schedule?')) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.carBatteryService.deleteChargingSchedule(this.deviceId, scheduleId)
      .subscribe({
        next: () => {
          this.successMessage = 'Schedule deleted successfully';
          this.isLoading = false;
          this.loadChargingSchedules();
          setTimeout(() => this.successMessage = '', 3000);
        },
        error: (error) => {
          this.errorMessage = 'Failed to delete schedule';
          this.isLoading = false;
          console.error('Error deleting schedule:', error);
        }
      });
  }

  getTodayDate(): string {
    const today = new Date();
    return today.toISOString().split('T')[0];
  }

  getScheduleTimeString(schedule: ChargingSchedule): string {
    if (!schedule.scheduledTime) {
      return 'Invalid Time';
    }

    // Ensure scheduledTime is a Date object
    const dateObj = schedule.scheduledTime instanceof Date
      ? schedule.scheduledTime
      : new Date(schedule.scheduledTime);

    if (isNaN(dateObj.getTime())) {
      return 'Invalid Time';
    }

    return this.carBatteryService.convertUTCToNZDateTime(dateObj);
  }

  getBatteryLevelClass(): string {
    if (!this.batteryStatus) return 'battery-unknown';

    const level = this.batteryStatus.batteryLevel;
    if (level >= 80) return 'battery-high';
    if (level >= 40) return 'battery-medium';
    if (level >= 20) return 'battery-low';
    return 'battery-critical';
  }

  getBatteryIcon(): string {
    if (!this.batteryStatus) return '🔋';

    const level = this.batteryStatus.batteryLevel;
    if (this.batteryStatus.isCharging) return '⚡';
    if (level >= 80) return '🔋';
    if (level >= 40) return '🔋';
    if (level >= 20) return '🪫';
    return '🪫';
  }
}
