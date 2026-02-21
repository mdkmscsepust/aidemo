import { Component, inject, OnInit, signal } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { RestaurantListDto } from '../../../core/models/restaurant.model';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { StarRatingComponent } from '../../../shared/components/star-rating/star-rating.component';

@Component({
  selector: 'app-admin-restaurants',
  standalone: true,
  imports: [
    MatTableModule, MatButtonModule, MatIconModule, MatChipsModule,
    LoadingSpinnerComponent, PageHeaderComponent, StarRatingComponent
  ],
  template: `
    <div class="page">
      <app-page-header title="Restaurant Management" subtitle="Approve and manage all restaurants" />
      @if (loading()) {
        <app-loading-spinner />
      } @else {
        <table mat-table [dataSource]="restaurants()">
          <ng-container matColumnDef="name">
            <th mat-header-cell *matHeaderCellDef>Name</th>
            <td mat-cell *matCellDef="let r">{{ r.name }}</td>
          </ng-container>
          <ng-container matColumnDef="city">
            <th mat-header-cell *matHeaderCellDef>City</th>
            <td mat-cell *matCellDef="let r">{{ r.city }}</td>
          </ng-container>
          <ng-container matColumnDef="cuisine">
            <th mat-header-cell *matHeaderCellDef>Cuisine</th>
            <td mat-cell *matCellDef="let r">{{ r.cuisineType }}</td>
          </ng-container>
          <ng-container matColumnDef="rating">
            <th mat-header-cell *matHeaderCellDef>Rating</th>
            <td mat-cell *matCellDef="let r"><app-star-rating [rating]="r.avgRating" /></td>
          </ng-container>
          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let r">
              <span [class]="r.isApproved ? 'chip-approved' : 'chip-pending'">
                {{ r.isApproved ? 'Approved' : 'Pending' }}
              </span>
            </td>
          </ng-container>
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Actions</th>
            <td mat-cell *matCellDef="let r">
              @if (!r.isApproved) {
                <button mat-flat-button color="primary" (click)="approve(r)">Approve</button>
              }
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="columns"></tr>
          <tr mat-row *matRowDef="let row; columns: columns;"></tr>
        </table>
      }
    </div>
  `,
  styles: [`
    .page { max-width: 1100px; margin: 0 auto; padding: 32px 24px; }
    table { width: 100%; }
    .chip-approved { background: #dcfce7; color: #16a34a; padding: 4px 10px; border-radius: 999px; font-size: 12px; }
    .chip-pending { background: #fef9c3; color: #ca8a04; padding: 4px 10px; border-radius: 999px; font-size: 12px; }
  `]
})
export class AdminRestaurantsComponent implements OnInit {
  private readonly restaurantService = inject(RestaurantService);
  private readonly snackBar = inject(MatSnackBar);

  restaurants = signal<RestaurantListDto[]>([]);
  loading = signal(true);
  columns = ['name', 'city', 'cuisine', 'rating', 'status', 'actions'];

  ngOnInit(): void {
    this.restaurantService.getAll({ pageSize: 100 }).subscribe({
      next: res => { this.restaurants.set(res.data?.items ?? []); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  approve(r: RestaurantListDto): void {
    this.restaurantService.approve(r.id).subscribe({
      next: () => {
        this.snackBar.open(`${r.name} approved!`, 'OK', { duration: 3000 });
        this.restaurants.update(list => list.map(x => x.id === r.id ? { ...x, isApproved: true } : x));
      },
      error: err => this.snackBar.open(err.error?.message ?? 'Failed to approve.', 'Close', { duration: 4000 })
    });
  }
}
