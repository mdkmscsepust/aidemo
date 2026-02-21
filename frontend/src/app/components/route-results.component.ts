import { Component } from '@angular/core';
import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { RouteStoreService } from '../services/route-store.service';
import { RouteItem } from '../models/route.models';

@Component({
  selector: 'app-route-results',
  standalone: true,
  imports: [AsyncPipe, NgFor, NgIf],
  template: `
    <section class="results">
      <header>
        <h3>Routes</h3>
        <span *ngIf="(state$ | async)?.loading" class="badge">Loading...</span>
      </header>

      <div *ngIf="(state$ | async)?.error as error" class="error">
        {{ error }}
      </div>

      <div class="cards">
        <button
          *ngFor="let route of (state$ | async)?.routes"
          class="card"
          [class.active]="route.id === (state$ | async)?.selectedRouteId"
          (click)="select(route)"
        >
          <div class="row">
            <strong>Route {{ route.rank }}</strong>
            <span>{{ formatDuration(route.etaSeconds) }}</span>
          </div>
          <div class="meta">
            <span>{{ formatDistance(route.distanceMeters) }}</span>
            <span *ngIf="route.trafficPenaltySeconds > 0">
              +{{ formatDuration(route.trafficPenaltySeconds) }} traffic
            </span>
          </div>
        </button>
      </div>
    </section>
  `,
  styleUrls: ['./route-results.component.scss']
})
export class RouteResultsComponent {
  readonly state$ = this.store.state$;

  constructor(private readonly store: RouteStoreService) {}

  select(route: RouteItem): void {
    this.store.selectRoute(route.id);
  }

  formatDuration(seconds: number): string {
    const mins = Math.round(seconds / 60);
    if (mins < 60) {
      return `${mins} min`;
    }
    const hours = Math.floor(mins / 60);
    const rem = mins % 60;
    return `${hours}h ${rem}m`;
  }

  formatDistance(meters: number): string {
    if (meters < 1000) {
      return `${meters} m`;
    }
    return `${(meters / 1000).toFixed(1)} km`;
  }
}
