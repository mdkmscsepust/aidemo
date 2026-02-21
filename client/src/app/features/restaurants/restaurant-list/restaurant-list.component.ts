import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { RestaurantListDto } from '../../../core/models/restaurant.model';
import { PaginatedResult } from '../../../core/models/api-response.model';
import { PriceTierPipe } from '../../../shared/pipes/price-tier.pipe';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-restaurant-list',
  standalone: true,
  imports: [DecimalPipe, ReactiveFormsModule, RouterLink, MatPaginatorModule, PriceTierPipe, LoadingSpinnerComponent, EmptyStateComponent],
  template: `
    <div class="page">

      <!-- FILTER BAR -->
      <div class="filter-bar">
        <input class="search-input" type="text" [formControl]="searchCtrl"
               placeholder="Restaurant name, cuisine…" (keyup.enter)="applyFilters()">
        <input class="search-input city-input" type="text" [formControl]="cityCtrl"
               placeholder="City" (keyup.enter)="applyFilters()">
        <button class="btn-search" (click)="applyFilters()">Search</button>
      </div>

      <!-- CUISINE CHIPS -->
      <div class="chips">
        @for (chip of cuisineChips; track chip) {
          <div class="chip" [class.active]="activeCuisine === chip" (click)="setCuisine(chip)">{{ chip }}</div>
        }
      </div>

      <!-- SECTION HEADER -->
      <div class="section-hdr">
        <div class="divider-line"></div>
        <span class="section-label">
          @if (result()?.totalCount) { {{ result()!.totalCount }} restaurants found }
          @else { Available tonight }
        </span>
        <div class="divider-line"></div>
      </div>

      @if (loading()) {
        <app-loading-spinner />
      } @else if ((result()?.items?.length ?? 0) === 0) {
        <app-empty-state icon="restaurant" title="No restaurants found" subtitle="Try adjusting your filters" />
      } @else {
        <div class="grid">
          @for (r of result()?.items ?? []; track r.id) {
            <div class="rcard" [routerLink]="['/restaurants', r.id]">
              <div class="card-img" [style.background]="cuisineGradient(r.cuisineType)">
                <span class="card-emoji">{{ cuisineEmoji(r.cuisineType) }}</span>
                <div class="card-fav" (click)="$event.stopPropagation()">♡</div>
              </div>
              <div class="card-body">
                <div class="card-cuisine">{{ r.cuisineType ?? 'Restaurant' }}</div>
                <div class="card-name">{{ r.name }}</div>
                <div class="card-meta-row">
                  <span class="stars">★ {{ r.avgRating | number:'1.1-1' }}</span>
                  <span class="review-ct">({{ r.reviewCount }})</span>
                  <span class="price">{{ r.priceTier | priceTier }}</span>
                </div>
                <div class="card-loc">📍 {{ r.city }} · {{ r.addressLine1 }}</div>
                <div class="slot-row">
                  @for (s of slotTimes; track s.time) {
                    <span class="slot-pill"
                          [class.slot-selected]="s.time === selectedTime"
                          (click)="$event.stopPropagation(); goBook(r.id, s.time)">
                      {{ s.label }}
                    </span>
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
          [pageSizeOptions]="[6, 12, 24]"
          (page)="onPage($event)"
          showFirstLastButtons />
      }
    </div>
  `,
  styles: [`
    .page { max-width: 1280px; margin: 0 auto; padding: 32px 40px 60px; }

    .filter-bar {
      display: flex; gap: 10px; margin-bottom: 20px; flex-wrap: wrap;
    }
    .search-input {
      flex: 1; min-width: 200px; padding: 11px 14px;
      border: 1px solid var(--border); border-radius: 6px;
      font-family: 'DM Sans', sans-serif; font-size: 14px;
      background: var(--card-bg); outline: none; transition: border-color .2s;
    }
    .search-input:focus { border-color: var(--wine); }
    .city-input { flex: 0 0 180px; }
    .btn-search {
      padding: 11px 24px; background: var(--wine); color: white; border: none;
      border-radius: 6px; font-family: 'DM Sans', sans-serif;
      font-size: 14px; font-weight: 500; cursor: pointer; transition: background .2s;
    }
    .btn-search:hover { background: var(--wine-light); }

    .chips { display: flex; gap: 8px; flex-wrap: wrap; margin-bottom: 32px; }
    .chip {
      padding: 7px 16px; border: 1px solid var(--border); border-radius: 100px;
      font-size: 13px; color: var(--warm-gray); cursor: pointer; transition: all .2s;
      background: var(--card-bg);
    }
    .chip:hover { border-color: var(--ink); color: var(--ink); }
    .chip.active { background: var(--ink); border-color: var(--ink); color: white; }

    .section-hdr { display: flex; align-items: center; gap: 16px; margin-bottom: 28px; }
    .divider-line { flex: 1; height: 1px; background: var(--border); }
    .section-label { font-family: 'Playfair Display', serif; font-size: 18px; white-space: nowrap; }

    .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 26px; margin-bottom: 28px; }

    .rcard {
      background: var(--card-bg); border: 1px solid var(--border);
      border-radius: var(--radius); overflow: hidden;
      cursor: pointer; transition: transform .25s, box-shadow .25s;
    }
    .rcard:hover { transform: translateY(-4px); box-shadow: var(--shadow-lg); }

    .card-img {
      height: 196px; display: flex; align-items: center; justify-content: center;
      position: relative; overflow: hidden;
    }
    .card-emoji { font-size: 58px; }
    .card-fav {
      position: absolute; top: 12px; right: 13px;
      width: 30px; height: 30px; border-radius: 50%; background: white;
      display: flex; align-items: center; justify-content: center;
      font-size: 14px; cursor: pointer; box-shadow: 0 2px 8px rgba(0,0,0,.12);
      transition: transform .2s;
    }
    .card-fav:hover { transform: scale(1.18); }

    .card-body { padding: 20px; }
    .card-cuisine {
      font-size: 11px; font-weight: 500; letter-spacing: .14em;
      text-transform: uppercase; color: var(--gold); margin-bottom: 5px;
    }
    .card-name {
      font-family: 'Playfair Display', serif; font-size: 19px;
      color: var(--ink); margin-bottom: 8px;
    }
    .card-meta-row { display: flex; align-items: center; gap: 8px; margin-bottom: 6px; }
    .stars { font-size: 13px; font-weight: 500; color: var(--gold); }
    .review-ct { font-size: 12px; color: var(--warm-gray); }
    .price { font-size: 13px; color: var(--warm-gray); margin-left: auto; }
    .card-loc { font-size: 12px; color: var(--warm-gray); margin-bottom: 14px; }

    .slot-row { display: flex; gap: 6px; flex-wrap: wrap; }
    .slot-pill {
      padding: 5px 11px; border: 1px solid var(--wine); border-radius: 100px;
      font-size: 12px; color: var(--wine); font-weight: 500;
      transition: all .18s; cursor: pointer;
    }
    .slot-pill:hover { background: var(--wine); color: white; }
    .slot-pill.slot-selected { background: var(--wine); color: white; font-weight: 600; }
  `]
})
export class RestaurantListComponent implements OnInit {
  private readonly restaurantService = inject(RestaurantService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  result = signal<PaginatedResult<RestaurantListDto> | null>(null);
  loading = signal(false);
  page = 1;
  pageSize = 12;
  activeCuisine = 'All';

  selectedTime = '19:00';
  selectedDate = '';
  selectedPartySize = 2;

  searchCtrl = this.fb.control('');
  cityCtrl = this.fb.control('');

  private timeToMinutes(t: string): number {
    const [h, m] = t.split(':').map(Number);
    return h * 60 + (m || 0);
  }
  private minutesToTime(mins: number): string {
    const h = Math.floor(mins / 60) % 24;
    const m = mins % 60;
    return `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}`;
  }
  formatTime(t: string): string {
    const [h, m] = t.split(':').map(Number);
    const ampm = h >= 12 ? 'PM' : 'AM';
    const h12 = h % 12 || 12;
    return `${h12}:${(m || 0).toString().padStart(2, '0')} ${ampm}`;
  }
  get slotTimes(): { time: string; label: string }[] {
    const base = this.timeToMinutes(this.selectedTime);
    return [-30, 0, 30]
      .map(offset => base + offset)
      .filter(m => m >= 0 && m < 24 * 60)
      .map(m => ({ time: this.minutesToTime(m), label: this.formatTime(this.minutesToTime(m)) }));
  }

  cuisineChips = ['All', 'Fine Dining', 'Italian', 'Japanese', 'French', 'Seafood', 'Vegetarian', 'Trending'];

  private readonly cuisineMap: Record<string, { gradient: string; emoji: string }> = {
    'Italian':  { gradient: 'linear-gradient(135deg,#1F1508,#4A3010)', emoji: '🍝' },
    'Japanese': { gradient: 'linear-gradient(135deg,#0D2137,#1A4A6B)', emoji: '🍣' },
    'American': { gradient: 'linear-gradient(135deg,#2D1B0E,#5C3317)', emoji: '🍔' },
    'French':   { gradient: 'linear-gradient(135deg,#2A1B35,#543070)', emoji: '🥐' },
    'Mexican':  { gradient: 'linear-gradient(135deg,#1C2A1A,#3B5E35)', emoji: '🌮' },
    'Seafood':  { gradient: 'linear-gradient(135deg,#0F1F2A,#0D3B55)', emoji: '🦞' },
    'Chinese':  { gradient: 'linear-gradient(135deg,#2A0A0A,#6B1515)', emoji: '🥢' },
    'Indian':   { gradient: 'linear-gradient(135deg,#2A1A0A,#6B3A0A)', emoji: '🍛' },
    'Thai':     { gradient: 'linear-gradient(135deg,#1A2A1A,#4A6B1A)', emoji: '🍜' },
  };

  cuisineGradient(c?: string): string {
    return this.cuisineMap[c ?? '']?.gradient ?? 'linear-gradient(135deg,#1A1410,#3D2F28)';
  }
  cuisineEmoji(c?: string): string {
    return this.cuisineMap[c ?? '']?.emoji ?? '🍽️';
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe(p => {
      if (p['search'])    this.searchCtrl.setValue(p['search']);
      if (p['cuisine'])   { this.searchCtrl.setValue(p['cuisine']); this.activeCuisine = p['cuisine']; }
      if (p['city'])      this.cityCtrl.setValue(p['city']);
      if (p['time'])      this.selectedTime = p['time'];
      if (p['date'])      this.selectedDate = p['date'];
      if (p['partySize']) this.selectedPartySize = +p['partySize'];
      this.loadRestaurants();
    });
  }

  loadRestaurants(): void {
    this.loading.set(true);
    const cuisine = this.activeCuisine !== 'All' ? this.activeCuisine : undefined;
    this.restaurantService.getAll({
      search: this.searchCtrl.value || cuisine || undefined,
      city: this.cityCtrl.value || undefined,
      page: this.page, pageSize: this.pageSize
    }).subscribe({
      next: res => { this.result.set(res.data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  applyFilters(): void { this.page = 1; this.loadRestaurants(); }

  setCuisine(c: string): void {
    this.activeCuisine = c;
    if (c !== 'All') this.searchCtrl.setValue(c);
    else this.searchCtrl.setValue('');
    this.applyFilters();
  }

  goBook(id: string, time: string): void {
    this.router.navigate(['/restaurants', id], {
      queryParams: {
        time,
        ...(this.selectedDate      ? { date: this.selectedDate }           : {}),
        ...(this.selectedPartySize ? { partySize: this.selectedPartySize } : {})
      }
    });
  }

  onPage(e: PageEvent): void {
    this.page = e.pageIndex + 1; this.pageSize = e.pageSize; this.loadRestaurants();
  }
}
