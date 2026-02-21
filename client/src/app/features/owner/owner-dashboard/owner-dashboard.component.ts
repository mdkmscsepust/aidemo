import { Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { ReservationService } from '../../../core/services/reservation.service';
import { RestaurantListDto } from '../../../core/models/restaurant.model';
import { ReservationDto } from '../../../core/models/reservation.model';
import { PaginatedResult } from '../../../core/models/api-response.model';
import { StarRatingComponent } from '../../../shared/components/star-rating/star-rating.component';
import { ReservationStatusPipe } from '../../../shared/pipes/reservation-status.pipe';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { DatePipe } from '@angular/common';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-owner-dashboard',
  standalone: true,
  imports: [
    RouterLink,
    MatCardModule, MatButtonModule, MatIconModule,
    StarRatingComponent, LoadingSpinnerComponent,
    EmptyStateComponent, PageHeaderComponent
  ],
  template: `
    <div class="page">
      <app-page-header title="Owner Dashboard" subtitle="Manage your restaurants and reservations" />

      @if (loadingRestaurants()) {
        <app-loading-spinner />
      } @else if (restaurants().length === 0) {
        <app-empty-state icon="store" title="No restaurants yet" subtitle="Add your first restaurant to get started" />
        <div class="center">
          <button mat-flat-button color="primary" routerLink="/owner/restaurants/new">Add Restaurant</button>
        </div>
      } @else {
        <div class="restaurant-grid">
          @for (r of restaurants(); track r.id) {
            <mat-card class="restaurant-card">
              <mat-card-header>
                <mat-card-title>{{ r.name }}</mat-card-title>
                <mat-card-subtitle>{{ r.cuisineType }} · {{ r.city }}</mat-card-subtitle>
              </mat-card-header>
              <mat-card-content>
                <app-star-rating [rating]="r.avgRating" [count]="r.reviewCount" [showCount]="true" />
                <p class="approval" [class.approved]="r.isApproved">
                  <mat-icon>{{ r.isApproved ? 'check_circle' : 'pending' }}</mat-icon>
                  {{ r.isApproved ? 'Approved' : 'Pending approval' }}
                </p>
              </mat-card-content>
              <mat-card-actions>
                <button mat-button [routerLink]="['/owner/restaurants', r.id]">
                  <mat-icon>edit</mat-icon> Manage
                </button>
                <button mat-button [routerLink]="['/owner/restaurants', r.id, 'reservations']">
                  <mat-icon>event</mat-icon> Reservations
                </button>
              </mat-card-actions>
            </mat-card>
          }
          <mat-card class="add-card" routerLink="/owner/restaurants/new">
            <mat-card-content>
              <mat-icon>add_circle</mat-icon>
              <p>Add Restaurant</p>
            </mat-card-content>
          </mat-card>
        </div>
      }
    </div>
  `,
  styles: [`
    .page { max-width: 1100px; margin: 0 auto; padding: 32px 24px; }
    .restaurant-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 20px; }
    .approval { display: flex; align-items: center; gap: 4px; font-size: 13px; margin-top: 8px; color: #f59e0b; }
    .approval.approved { color: #16a34a; }
    .approval mat-icon { font-size: 16px; width: 16px; height: 16px; }
    .add-card { cursor: pointer; border: 2px dashed #d1d5db; display: flex; align-items: center; justify-content: center; min-height: 180px; }
    .add-card mat-card-content { display: flex; flex-direction: column; align-items: center; gap: 8px; color: #9ca3af; }
    .add-card mat-icon { font-size: 48px; width: 48px; height: 48px; }
    .add-card:hover { border-color: #6366f1; color: #6366f1; }
    .center { text-align: center; margin-top: 24px; }
  `]
})
export class OwnerDashboardComponent implements OnInit {
  private readonly restaurantService = inject(RestaurantService);

  restaurants = signal<RestaurantListDto[]>([]);
  loadingRestaurants = signal(true);

  ngOnInit(): void {
    this.restaurantService.getAll({ pageSize: 50 }).subscribe({
      next: res => { this.restaurants.set(res.data?.items ?? []); this.loadingRestaurants.set(false); },
      error: () => this.loadingRestaurants.set(false)
    });
  }
}
