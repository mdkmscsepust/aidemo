import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouteStoreService } from '../services/route-store.service';
import { RouteRequest } from '../models/route.models';

@Component({
  selector: 'app-route-planner-panel',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <section class="planner">
      <h2>Plan your route</h2>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <label>
          From
          <input formControlName="from" placeholder="Shantinagar, Dhaka or 23.78, 90.41" />
        </label>
        <button type="button" class="ghost" (click)="useMyLocation()">Use my location</button>

        <label>
          To
          <input formControlName="to" placeholder="Uttara, Dhaka or 23.87, 90.40" />
        </label>

        <div class="row">
          <label class="inline">
            Alternatives
            <input type="number" min="0" max="3" formControlName="alternatives" />
          </label>
          <label class="toggle">
            <input type="checkbox" formControlName="traffic" />
            Traffic-aware ETA
          </label>
        </div>

        <div class="row">
          <label class="toggle">
            <input type="checkbox" formControlName="avoidTolls" />
            Avoid tolls
          </label>
          <label class="toggle">
            <input type="checkbox" formControlName="avoidHighways" />
            Avoid highways
          </label>
        </div>

        <button class="primary" type="submit" [disabled]="form.invalid">Find routes</button>
      </form>
    </section>
  `,
  styleUrls: ['./route-planner-panel.component.scss']
})
export class RoutePlannerPanelComponent implements OnInit {
  form = this.fb.group({
    from: ['', Validators.required],
    to: ['', Validators.required],
    alternatives: [2, [Validators.min(0), Validators.max(3)]],
    traffic: [true],
    avoidTolls: [false],
    avoidHighways: [false]
  });

  constructor(private readonly fb: FormBuilder, private readonly store: RouteStoreService) {}

  ngOnInit(): void {
    this.form.patchValue({ alternatives: 2, traffic: true });
  }

  submit(): void {
    if (this.form.invalid) {
      return;
    }

    const request: RouteRequest = {
      from: this.parseLocation(this.form.value.from ?? ''),
      to: this.parseLocation(this.form.value.to ?? ''),
      transportMode: 'car',
      alternatives: this.form.value.alternatives ?? 2,
      traffic: !!this.form.value.traffic,
      avoidTolls: !!this.form.value.avoidTolls,
      avoidHighways: !!this.form.value.avoidHighways
    };

    this.store.planRoute(request);
  }

  useMyLocation(): void {
    if (!navigator.geolocation) {
      return;
    }

    navigator.geolocation.getCurrentPosition(position => {
      const { latitude, longitude } = position.coords;
      this.form.patchValue({ from: `${latitude.toFixed(6)}, ${longitude.toFixed(6)}` });
    });
  }

  private parseLocation(value: string) {
    const trimmed = value.trim();
    const match = trimmed.match(/^(-?\d+(?:\.\d+)?)\s*,\s*(-?\d+(?:\.\d+)?)$/);
    if (match) {
      return { lat: Number(match[1]), lng: Number(match[2]) };
    }
    return { text: trimmed };
  }
}
