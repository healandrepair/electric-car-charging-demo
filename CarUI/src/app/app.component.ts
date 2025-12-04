import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CarDashboardComponent } from './components/car-dashboard/car-dashboard.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, CarDashboardComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'Car Battery Management';
}
