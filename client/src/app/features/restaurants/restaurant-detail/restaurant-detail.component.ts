import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { ReservationService } from '../../../core/services/reservation.service';
import { ReviewService } from '../../../core/services/review.service';
import { AuthService } from '../../../core/services/auth.service';
import { RestaurantDetailDto } from '../../../core/models/restaurant.model';
import { AvailableSlotDto } from '../../../core/models/reservation.model';
import { ReviewDto } from '../../../core/models/review.model';
import { PriceTierPipe } from '../../../shared/pipes/price-tier.pipe';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-restaurant-detail',
  standalone: true,
  imports: [
    DatePipe, DecimalPipe, ReactiveFormsModule, FormsModule,
    PriceTierPipe, LoadingSpinnerComponent
  ],
  template: `
    @if (loading()) {
      <app-loading-spinner />
    } @else if (restaurant()) {
      <!-- HERO BANNER -->
      <div class="hero-banner" [style.background]="cuisineGradient(restaurant()!.cuisineType)">
        <span class="hero-emoji">{{ cuisineEmoji(restaurant()!.cuisineType) }}</span>
      </div>

      <div class="page">
        <div class="content">

          <!-- MAIN INFO -->
          <div class="main">
            <div class="eyebrow">{{ restaurant()!.cuisineType ?? 'Restaurant' }}</div>
            <h1>{{ restaurant()!.name }}</h1>
            <div class="meta-row">
              <span class="stars">★ {{ restaurant()!.avgRating | number:'1.1-1' }}</span>
              <span class="reviews-ct">({{ restaurant()!.reviewCount }} reviews)</span>
              <span class="price">{{ restaurant()!.priceTier | priceTier }}</span>
              <span class="location">📍 {{ restaurant()!.city }}{{ restaurant()!.state ? ', ' + restaurant()!.state : '' }}</span>
            </div>
            @if (restaurant()!.description) {
              <p class="description">{{ restaurant()!.description }}</p>
            }

            <div class="divider"></div>

            <h2>Hours</h2>
            <div class="hours-grid">
              @for (h of restaurant()!.openingHours; track h.id) {
                <div class="hour-row">
                  <span class="day">{{ h.dayOfWeek }}</span>
                  <span class="hours-val">{{ h.isClosed ? 'Closed' : h.openTime + ' – ' + h.closeTime }}</span>
                </div>
              }
            </div>

            <div class="divider"></div>

            <h2>Guest Reviews</h2>
            @if (reviews().length === 0) {
              <p class="muted">No reviews yet — be the first to dine here.</p>
            } @else {
              @for (rev of reviews(); track rev.id) {
                <div class="review">
                  <div class="review-header">
                    <strong>{{ rev.customerName }}</strong>
                    <span class="rev-stars">{{ '★'.repeat(rev.rating) }}</span>
                    <span class="rev-date">{{ rev.createdAt | date:'mediumDate' }}</span>
                  </div>
                  @if (rev.comment) { <p class="rev-body">{{ rev.comment }}</p> }
                </div>
              }
            }
          </div>

          <!-- BOOKING PANEL -->
          <aside class="booking-panel">
            <div class="book-card">
              <div class="book-title">Make a Reservation</div>
              <p class="book-sub">Free cancellation · 2 hr advance notice required</p>

              <div class="form-group">
                <label>Date</label>
                <input type="date" [formControl]="bookForm.controls.date" [min]="today" (change)="loadSlots()">
              </div>
              <div class="form-group">
                <label>Party size</label>
                <select [formControl]="bookForm.controls.partySize" (change)="loadSlots()">
                  @for (n of [1,2,3,4,5,6,7,8]; track n) {
                    <option [value]="n">{{ n }} {{ n === 1 ? 'guest' : 'guests' }}</option>
                  }
                </select>
              </div>

              @if (slotsLoading()) {
                <div class="slots-loading">Loading availability…</div>
              } @else if (slots().length > 0) {
                <div class="form-group">
                  <label>Available times</label>
                  <div class="slots-grid">
                    @for (slot of slots(); track slot.slotTime + slot.tableId) {
                      <div class="slot-pill" [class.active]="selectedSlot() === slot"
                           (click)="selectSlot(slot)">{{ slot.slotTime }}</div>
                    }
                  </div>
                </div>
              } @else if (slotsLoaded()) {
                <p class="muted-sm">No availability for this date.</p>
              }

              @if (selectedSlot()) {
                <div class="form-group">
                  <label>Special requests (optional)</label>
                  <textarea [(ngModel)]="specialRequests" rows="2" placeholder="Window seat, allergies…"></textarea>
                </div>
                <button class="btn-confirm" (click)="book()" [disabled]="booking()">
                  {{ booking() ? 'Confirming…' : 'Confirm Reservation' }}
                </button>
              }

              @if (!slotsLoaded()) {
                <p class="hint">Select a date and party size to see availability</p>
              }
            </div>
          </aside>

        </div>
      </div>
    }
  `,
  styles: [`
    .hero-banner {
      height: 260px; display: flex; align-items: center; justify-content: center;
    }
    .hero-emoji { font-size: 80px; }

    .page { max-width: 1280px; margin: 0 auto; padding: 0 40px 60px; }
    .content { display: grid; grid-template-columns: 1fr 380px; gap: 40px; margin-top: 32px; }

    /* Main */
    .eyebrow {
      font-size: 11px; font-weight: 500; letter-spacing: .16em; text-transform: uppercase;
      color: var(--gold); margin-bottom: 8px;
    }
    h1 {
      font-family: 'Playfair Display', serif; font-size: 36px;
      color: var(--ink); margin: 0 0 12px;
    }
    h2 { font-family: 'Playfair Display', serif; font-size: 22px; margin: 0 0 14px; }
    .meta-row { display: flex; align-items: center; flex-wrap: wrap; gap: 10px; margin-bottom: 16px; }
    .stars { font-size: 14px; color: var(--gold); font-weight: 500; }
    .reviews-ct { font-size: 13px; color: var(--warm-gray); }
    .price { font-size: 13px; color: var(--warm-gray); }
    .location { font-size: 13px; color: var(--warm-gray); }
    .description { font-size: 15px; line-height: 1.7; color: var(--warm-gray); margin: 0 0 24px; }
    .divider { height: 1px; background: var(--border); margin: 24px 0; }
    .hours-grid { display: grid; gap: 8px; font-size: 14px; }
    .hour-row { display: flex; gap: 12px; }
    .day { font-weight: 500; width: 110px; color: var(--ink); }
    .hours-val { color: var(--warm-gray); }
    .review { padding: 14px 0; border-bottom: 1px solid var(--border); }
    .review-header { display: flex; align-items: center; gap: 10px; margin-bottom: 6px; }
    .rev-stars { color: var(--gold); font-size: 13px; }
    .rev-date { font-size: 12px; color: var(--warm-gray); margin-left: auto; }
    .rev-body { font-size: 14px; color: var(--warm-gray); line-height: 1.6; margin: 0; }
    .muted { color: var(--warm-gray); font-size: 14px; }

    /* Booking panel */
    .booking-panel { position: sticky; top: 88px; align-self: start; }
    .book-card {
      background: var(--card-bg); border: 1px solid var(--border);
      border-radius: var(--radius); padding: 32px; box-shadow: var(--shadow);
    }
    .book-title { font-family: 'Playfair Display', serif; font-size: 20px; margin-bottom: 4px; }
    .book-sub { font-size: 12px; color: var(--warm-gray); margin: 0 0 22px; }
    .form-group { margin-bottom: 16px; }
    label {
      display: block; font-size: 11px; font-weight: 500;
      letter-spacing: .12em; text-transform: uppercase;
      color: var(--warm-gray); margin-bottom: 7px;
    }
    input[type="date"], select, textarea {
      width: 100%; padding: 11px 13px;
      border: 1px solid var(--border); border-radius: 6px;
      font-family: 'DM Sans', sans-serif; font-size: 14px; color: var(--ink);
      background: var(--cream); outline: none; appearance: none; transition: border-color .2s;
    }
    input:focus, select:focus, textarea:focus { border-color: var(--wine); }
    textarea { resize: vertical; min-height: 72px; }
    .slots-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 7px; }
    .slot-pill {
      padding: 9px 4px; border: 1px solid var(--border); border-radius: 6px;
      text-align: center; font-size: 13px; cursor: pointer;
      transition: all .18s; background: var(--cream);
    }
    .slot-pill:hover { border-color: var(--wine); color: var(--wine); }
    .slot-pill.active { background: var(--wine); border-color: var(--wine); color: white; font-weight: 500; }
    .btn-confirm {
      width: 100%; background: var(--wine); color: white; border: none; cursor: pointer;
      padding: 14px; border-radius: 6px; font-family: 'DM Sans', sans-serif;
      font-size: 14px; font-weight: 500; letter-spacing: .04em; margin-top: 8px;
      transition: background .2s;
    }
    .btn-confirm:hover:not([disabled]) { background: var(--wine-light); }
    .btn-confirm[disabled] { opacity: .6; cursor: not-allowed; }
    .slots-loading { font-size: 13px; color: var(--warm-gray); padding: 12px 0; }
    .muted-sm { font-size: 13px; color: var(--warm-gray); margin: 0 0 12px; }
    .hint { font-size: 12px; color: var(--warm-gray); text-align: center; margin-top: 14px; }

    @media (max-width: 900px) { .content { grid-template-columns: 1fr; } .booking-panel { position: static; } }
  `]
})
export class RestaurantDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly restaurantService = inject(RestaurantService);
  private readonly reservationService = inject(ReservationService);
  private readonly reviewService = inject(ReviewService);
  private readonly auth = inject(AuthService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly fb = inject(FormBuilder);

  private readonly cuisineMap: Record<string, { gradient: string; emoji: string }> = {
    'Italian':    { gradient: 'linear-gradient(135deg,#1F1508,#4A3010)', emoji: '🍝' },
    'Japanese':   { gradient: 'linear-gradient(135deg,#0D2137,#1A4A6B)', emoji: '🍣' },
    'American':   { gradient: 'linear-gradient(135deg,#2D1B0E,#5C3317)', emoji: '🍔' },
    'French':     { gradient: 'linear-gradient(135deg,#2A1B35,#543070)', emoji: '🥐' },
    'Mexican':    { gradient: 'linear-gradient(135deg,#1C2A1A,#3B5E35)', emoji: '🌮' },
    'Seafood':    { gradient: 'linear-gradient(135deg,#0F1F2A,#0D3B55)', emoji: '🦞' },
    'Chinese':    { gradient: 'linear-gradient(135deg,#2A0A0A,#6B1515)', emoji: '🥢' },
    'Indian':     { gradient: 'linear-gradient(135deg,#2A1A0A,#6B3A0A)', emoji: '🍛' },
    'Thai':       { gradient: 'linear-gradient(135deg,#1A2A1A,#4A6B1A)', emoji: '🍜' },
    'Fine Dining':{ gradient: 'linear-gradient(135deg,#1A1410,#3D2F28)', emoji: '🍽️' },
  };

  cuisineGradient(c?: string): string {
    return this.cuisineMap[c ?? '']?.gradient ?? 'linear-gradient(135deg,#1A1410,#3D2F28)';
  }
  cuisineEmoji(c?: string): string {
    return this.cuisineMap[c ?? '']?.emoji ?? '🍽️';
  }

  restaurant = signal<RestaurantDetailDto | null>(null);
  reviews = signal<ReviewDto[]>([]);
  slots = signal<AvailableSlotDto[]>([]);
  selectedSlot = signal<AvailableSlotDto | null>(null);
  loading = signal(true);
  slotsLoading = signal(false);
  slotsLoaded = signal(false);
  booking = signal(false);
  specialRequests = '';
  today = new Date().toISOString().split('T')[0];

  bookForm = this.fb.group({
    date: [this.today, Validators.required],
    partySize: [2, [Validators.required, Validators.min(1), Validators.max(20)]]
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.restaurantService.getById(id).subscribe({
      next: res => { this.restaurant.set(res.data); this.loading.set(false); },
      error: () => { this.loading.set(false); this.router.navigate(['/restaurants']); }
    });
    this.reviewService.getForRestaurant(id).subscribe({
      next: res => this.reviews.set(res.data?.items ?? [])
    });
  }

  loadSlots(): void {
    const { date, partySize } = this.bookForm.value;
    if (!date || !partySize) return;
    const id = this.route.snapshot.paramMap.get('id')!;
    this.slotsLoading.set(true);
    this.slots.set([]);
    this.selectedSlot.set(null);
    this.restaurantService.getAvailability(id, date, partySize).subscribe({
      next: res => { this.slots.set(res.data ?? []); this.slotsLoading.set(false); this.slotsLoaded.set(true); },
      error: () => { this.slotsLoading.set(false); this.slotsLoaded.set(true); }
    });
  }

  selectSlot(slot: AvailableSlotDto): void {
    this.selectedSlot.set(slot);
  }

  book(): void {
    if (!this.auth.isLoggedIn()) { this.router.navigate(['/auth/login']); return; }
    const slot = this.selectedSlot();
    const { date, partySize } = this.bookForm.value;
    if (!slot || !date || !partySize) return;
    this.booking.set(true);
    this.reservationService.create({
      restaurantId: this.restaurant()!.id,
      tableId: slot.tableId,
      reservationDate: date,
      slotTime: slot.slotTime,
      partySize,
      specialRequests: this.specialRequests || undefined
    }).subscribe({
      next: res => {
        this.snackBar.open(`Reservation confirmed! Code: ${res.data?.confirmationCode}`, 'View', { duration: 6000 })
          .onAction().subscribe(() => this.router.navigate(['/reservations']));
        this.booking.set(false);
        this.router.navigate(['/reservations']);
      },
      error: err => {
        this.snackBar.open(err.error?.message ?? 'Booking failed. Slot may no longer be available.', 'Close', { duration: 5000 });
        this.booking.set(false);
        this.loadSlots();
      }
    });
  }
}
