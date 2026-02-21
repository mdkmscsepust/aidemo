import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { ReservationService } from '../../../core/services/reservation.service';
import { ReservationDto } from '../../../core/models/reservation.model';
import { PaginatedResult } from '../../../core/models/api-response.model';
import { ReservationStatusPipe } from '../../../shared/pipes/reservation-status.pipe';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-my-reservations',
  standalone: true,
  imports: [
    DatePipe, RouterLink,
    MatDialogModule, MatPaginatorModule,
    ReservationStatusPipe, LoadingSpinnerComponent
  ],
  template: `
    <div class="page">

      <!-- PAGE HEADER -->
      <div class="page-header">
        <div class="header-inner">
          <div class="eyebrow">Your dining history</div>
          <h1>My Reservations</h1>
          <p class="header-sub">Manage your upcoming and past visits</p>
        </div>
        <a class="btn-browse" routerLink="/restaurants">Find a Table →</a>
      </div>

      @if (loading()) {
        <app-loading-spinner />
      } @else if ((result()?.items?.length ?? 0) === 0) {
        <!-- EMPTY STATE -->
        <div class="empty-state">
          <div class="empty-icon">🍽️</div>
          <h2>No reservations yet</h2>
          <p>Discover exceptional restaurants and book your first table.</p>
          <a class="btn-browse-lg" routerLink="/restaurants">Browse Restaurants</a>
        </div>
      } @else {
        <div class="res-list">
          @for (r of result()?.items ?? []; track r.id) {
            <div class="res-card">
              <div class="card-accent" [class]="getStatusClass(r.status)"></div>
              <div class="card-body">

                <div class="card-top">
                  <div>
                    <div class="restaurant-name">{{ r.restaurantName }}</div>
                    <div class="res-meta">
                      <span>Table {{ r.tableNumber }}</span>
                      <span class="dot">·</span>
                      <span>{{ r.partySize }} {{ r.partySize === 1 ? 'guest' : 'guests' }}</span>
                    </div>
                  </div>
                  <span class="status-pill" [class]="getStatusClass(r.status)">
                    {{ r.status | reservationStatus }}
                  </span>
                </div>

                <div class="card-datetime">
                  <span class="date-icon">📅</span>
                  <span>{{ r.reservationDate | date:'EEEE, MMMM d, y' }}</span>
                  <span class="time-sep">at</span>
                  <span class="time-val">{{ r.startTime }}</span>
                </div>

                <div class="card-footer">
                  <span class="conf-code">Code: <strong>{{ r.confirmationCode }}</strong></span>
                  @if (r.specialRequests) {
                    <span class="req-badge">{{ r.specialRequests }}</span>
                  }
                  @if (canCancel(r)) {
                    <button class="btn-cancel" (click)="cancel(r)">Cancel reservation</button>
                  }
                </div>

              </div>
            </div>
          }
        </div>

        <mat-paginator
          [length]="result()?.totalCount ?? 0"
          [pageSize]="pageSize"
          [pageIndex]="page - 1"
          [pageSizeOptions]="[5, 10, 20]"
          (page)="onPage($event)"
          showFirstLastButtons />
      }
    </div>
  `,
  styles: [`
    .page { max-width: 860px; margin: 0 auto; padding: 0 24px 60px; }

    /* Header */
    .page-header {
      display: flex; justify-content: space-between; align-items: flex-end;
      padding: 40px 0 32px; border-bottom: 1px solid var(--border); margin-bottom: 32px;
      flex-wrap: wrap; gap: 16px;
    }
    .eyebrow {
      font-size: 11px; font-weight: 500; letter-spacing: .16em; text-transform: uppercase;
      color: var(--gold); margin-bottom: 8px;
    }
    h1 { font-family: 'Playfair Display', serif; font-size: 32px; color: var(--ink); margin: 0 0 6px; }
    .header-sub { font-size: 14px; color: var(--warm-gray); margin: 0; }
    .btn-browse {
      padding: 11px 22px; background: var(--wine); color: white; border: none;
      border-radius: 6px; font-family: 'DM Sans', sans-serif; font-size: 13px;
      font-weight: 500; text-decoration: none; white-space: nowrap;
      transition: background .2s;
    }
    .btn-browse:hover { background: var(--wine-light); }

    /* Empty */
    .empty-state {
      text-align: center; padding: 80px 20px;
    }
    .empty-icon { font-size: 56px; margin-bottom: 20px; }
    .empty-state h2 { font-family: 'Playfair Display', serif; font-size: 24px; color: var(--ink); margin: 0 0 10px; }
    .empty-state p { font-size: 14px; color: var(--warm-gray); margin: 0 0 28px; }
    .btn-browse-lg {
      display: inline-block; padding: 13px 28px; background: var(--wine); color: white;
      border-radius: 6px; text-decoration: none; font-family: 'DM Sans', sans-serif;
      font-size: 14px; font-weight: 500; transition: background .2s;
    }
    .btn-browse-lg:hover { background: var(--wine-light); }

    /* Cards */
    .res-list { display: flex; flex-direction: column; gap: 14px; margin-bottom: 28px; }
    .res-card {
      display: flex; background: var(--card-bg); border: 1px solid var(--border);
      border-radius: var(--radius); overflow: hidden; box-shadow: var(--shadow-sm);
      transition: box-shadow .2s;
    }
    .res-card:hover { box-shadow: var(--shadow); }

    .card-accent {
      width: 4px; flex-shrink: 0;
    }
    .card-accent.status-confirmed { background: #16a34a; }
    .card-accent.status-pending   { background: #ca8a04; }
    .card-accent.status-completed { background: #1d4ed8; }
    .card-accent.status-cancelled { background: #dc2626; }
    .card-accent.status-noshow    { background: #9ca3af; }

    .card-body { flex: 1; padding: 22px 24px; }

    .card-top {
      display: flex; justify-content: space-between; align-items: flex-start;
      margin-bottom: 12px; gap: 12px;
    }
    .restaurant-name {
      font-family: 'Playfair Display', serif; font-size: 19px; color: var(--ink); margin-bottom: 4px;
    }
    .res-meta { font-size: 13px; color: var(--warm-gray); display: flex; gap: 6px; align-items: center; }
    .dot { color: var(--border); }

    .status-pill {
      padding: 4px 12px; border-radius: 100px; font-size: 11px; font-weight: 600;
      letter-spacing: .06em; text-transform: uppercase; white-space: nowrap;
    }
    .status-pill.status-confirmed { background: #dcfce7; color: #16a34a; }
    .status-pill.status-pending   { background: #fef9c3; color: #ca8a04; }
    .status-pill.status-completed { background: #dbeafe; color: #1d4ed8; }
    .status-pill.status-cancelled { background: #fee2e2; color: #dc2626; }
    .status-pill.status-noshow    { background: #f3f4f6; color: #6b7280; }

    .card-datetime {
      display: flex; align-items: center; gap: 7px;
      font-size: 14px; color: var(--ink); margin-bottom: 16px;
    }
    .date-icon { font-size: 14px; }
    .time-sep { color: var(--warm-gray); font-size: 13px; }
    .time-val { font-weight: 500; color: var(--wine); }

    .card-footer {
      display: flex; align-items: center; gap: 14px; flex-wrap: wrap;
      padding-top: 14px; border-top: 1px solid var(--border);
    }
    .conf-code { font-size: 12px; color: var(--warm-gray); }
    .conf-code strong { color: var(--ink); font-weight: 600; }
    .req-badge {
      flex: 1; font-size: 12px; color: var(--warm-gray);
      font-style: italic; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
    }
    .btn-cancel {
      margin-left: auto; padding: 7px 16px;
      border: 1px solid #dc2626; border-radius: 6px; background: transparent;
      color: #dc2626; font-family: 'DM Sans', sans-serif; font-size: 12px;
      font-weight: 500; cursor: pointer; transition: all .18s; white-space: nowrap;
    }
    .btn-cancel:hover { background: #dc2626; color: white; }
  `]
})
export class MyReservationsComponent implements OnInit {
  private readonly reservationService = inject(ReservationService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  result = signal<PaginatedResult<ReservationDto> | null>(null);
  loading = signal(true);
  page = 1;
  pageSize = 10;

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.reservationService.getMyReservations(this.page, this.pageSize).subscribe({
      next: res => { this.result.set(res.data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  canCancel(r: ReservationDto): boolean {
    if (r.status !== 'Confirmed' && r.status !== 'Pending' && r.status !== '1' && r.status !== '0') return false;
    const resDate = new Date(r.reservationDate + 'T' + r.startTime);
    return resDate.getTime() > Date.now() + 2 * 60 * 60 * 1000;
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      'Confirmed': 'status-confirmed', '1': 'status-confirmed',
      'Pending': 'status-pending', '0': 'status-pending',
      'Completed': 'status-completed', '2': 'status-completed',
      'CancelledByCustomer': 'status-cancelled', '3': 'status-cancelled',
      'CancelledByRestaurant': 'status-cancelled', '4': 'status-cancelled',
      'NoShow': 'status-noshow', '5': 'status-noshow'
    };
    return map[status] ?? 'status-pending';
  }

  cancel(r: ReservationDto): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Cancel Reservation', message: `Cancel your reservation at ${r.restaurantName}?`, confirmLabel: 'Yes, cancel' }
    });
    ref.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.reservationService.cancel(r.id).subscribe({
        next: () => { this.snackBar.open('Reservation cancelled.', 'OK', { duration: 3000 }); this.load(); },
        error: err => this.snackBar.open(err.error?.message ?? 'Could not cancel.', 'Close', { duration: 4000 })
      });
    });
  }

  onPage(e: PageEvent): void {
    this.page = e.pageIndex + 1;
    this.pageSize = e.pageSize;
    this.load();
  }
}
