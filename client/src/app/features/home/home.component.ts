import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [FormsModule],
  template: `
    <!-- HERO -->
    <section class="hero-section">
      <div class="hero-inner">

        <!-- Left: Title + stats -->
        <div class="hero-left">
          <div class="eyebrow">Your table awaits</div>
          <h1 class="hero-title">
            Reserve the<br><em>perfect evening</em><br>in seconds.
          </h1>
          <p class="hero-sub">
            Discover the city's finest restaurants and secure your table —
            from intimate bistros to celebrated tasting menus.
          </p>
          <div class="hero-stats">
            <div class="stat">
              <div class="stat-num">4,200+</div>
              <div class="stat-lbl">Restaurants</div>
            </div>
            <div class="stat">
              <div class="stat-num">1.2M</div>
              <div class="stat-lbl">Reservations / mo</div>
            </div>
            <div class="stat">
              <div class="stat-num">4.9★</div>
              <div class="stat-lbl">App Rating</div>
            </div>
          </div>
        </div>

        <!-- Right: Find a Table card -->
        <div class="find-card">
          <h3 class="card-title">Find a Table</h3>
          <p class="card-sub">Free cancellation on most reservations</p>

          <div class="form-group">
            <label>Location</label>
            <input type="text" [(ngModel)]="city" placeholder="City or neighborhood">
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Date</label>
              <input type="date" [(ngModel)]="date" [min]="today">
            </div>
            <div class="form-group">
              <label>Party Size</label>
              <select [(ngModel)]="partySize">
                @for (n of partySizes; track n) {
                  <option [value]="n">{{ n }} {{ n === 1 ? 'guest' : 'guests' }}</option>
                }
              </select>
            </div>
          </div>

          <div class="form-group">
            <label>Preferred Time</label>
            <div class="time-grid">
              @for (t of timeSlots; track t.time) {
                <div class="time-pill"
                     [class.active]="selectedTime === t.time"
                     [class.unavail]="!t.available"
                     (click)="t.available && setTime(t.time)">
                  {{ t.time }}
                </div>
              }
            </div>
          </div>

          <button class="btn-search" (click)="search()">Search Available Tables</button>
        </div>
      </div>
    </section>

    <!-- FEATURED BANNER -->
    <div class="container">
      <div class="banner">
        <div>
          <div class="banner-eyebrow">Tonight's Picks</div>
          <div class="banner-title">Celebrate over an<br>unforgettable meal</div>
          <div class="banner-sub">Handpicked restaurants for every occasion</div>
        </div>
        <button class="btn-banner" (click)="browse()">Browse Restaurants →</button>
      </div>
    </div>

    <!-- CUISINE SECTION -->
    <div class="container cuisine-section">
      <div class="section-header">
        <div class="divider-line"></div>
        <h2 class="section-title">Explore by cuisine</h2>
        <div class="divider-line"></div>
      </div>
      <div class="cuisine-grid">
        @for (c of cuisines; track c.name) {
          <button class="cuisine-card" (click)="browseCuisine(c.name)">
            <span class="cuisine-emoji">{{ c.emoji }}</span>
            <span class="cuisine-name">{{ c.name }}</span>
          </button>
        }
      </div>
    </div>
  `,
  styles: [`
    /* HERO */
    .hero-section { padding: 72px 40px 56px; }
    .hero-inner {
      max-width: 1280px; margin: 0 auto;
      display: grid; grid-template-columns: 1fr 420px; gap: 72px; align-items: start;
    }
    .eyebrow {
      font-size: 11px; font-weight: 500; letter-spacing: .2em; text-transform: uppercase;
      color: var(--gold); margin-bottom: 20px;
    }
    .hero-title {
      font-family: 'Playfair Display', serif;
      font-size: clamp(36px, 4.5vw, 60px); line-height: 1.1;
      color: var(--ink); margin: 0 0 20px;
    }
    .hero-title em { color: var(--wine); font-style: italic; }
    .hero-sub { font-size: 16px; color: var(--warm-gray); line-height: 1.7; max-width: 420px; margin: 0 0 40px; }
    .hero-stats { display: flex; gap: 40px; }
    .stat-num { font-family: 'Playfair Display', serif; font-size: 30px; color: var(--ink); }
    .stat-lbl { font-size: 12px; color: var(--warm-gray); letter-spacing: .05em; margin-top: 2px; }

    /* FIND A TABLE CARD */
    .find-card {
      background: var(--card-bg); border: 1px solid var(--border);
      border-radius: var(--radius); padding: 36px;
      box-shadow: var(--shadow); position: sticky; top: 88px;
    }
    .card-title { font-family: 'Playfair Display', serif; font-size: 20px; margin: 0 0 4px; }
    .card-sub { font-size: 13px; color: var(--warm-gray); margin: 0 0 24px; }
    .form-group { margin-bottom: 18px; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    label {
      display: block; font-size: 11px; font-weight: 500;
      letter-spacing: .12em; text-transform: uppercase;
      color: var(--warm-gray); margin-bottom: 7px;
    }
    input, select {
      width: 100%; padding: 11px 13px;
      border: 1px solid var(--border); border-radius: 6px;
      font-family: 'DM Sans', sans-serif; font-size: 14px; color: var(--ink);
      background: var(--cream); outline: none; appearance: none;
      transition: border-color .2s;
    }
    input:focus, select:focus { border-color: var(--wine); }
    .time-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 7px; }
    .time-pill {
      padding: 9px 4px; border: 1px solid var(--border); border-radius: 6px;
      text-align: center; font-size: 13px; cursor: pointer;
      transition: all .18s; background: var(--cream); color: var(--ink);
    }
    .time-pill:hover:not(.unavail) { border-color: var(--wine); color: var(--wine); }
    .time-pill.active { background: var(--wine); border-color: var(--wine); color: white; font-weight: 500; }
    .time-pill.unavail { opacity: .35; cursor: not-allowed; text-decoration: line-through; }
    .btn-search {
      width: 100%; background: var(--wine); color: white; border: none;
      cursor: pointer; padding: 14px; border-radius: 6px;
      font-family: 'DM Sans', sans-serif; font-size: 14px; font-weight: 500;
      letter-spacing: .04em; margin-top: 20px; transition: background .2s;
    }
    .btn-search:hover { background: var(--wine-light); }

    /* FEATURED BANNER */
    .container { max-width: 1280px; margin: 0 auto; padding: 0 40px; }
    .banner {
      border-radius: 16px;
      background: linear-gradient(135deg, var(--wine) 0%, #5C0F1C 100%);
      padding: 48px 56px; margin-bottom: 60px;
      display: grid; grid-template-columns: 1fr auto; gap: 32px; align-items: center;
      position: relative; overflow: hidden;
    }
    .banner::before {
      content: ''; position: absolute; top: -40px; right: -40px;
      width: 260px; height: 260px; border-radius: 50%;
      background: rgba(255,255,255,.04);
    }
    .banner-eyebrow {
      font-size: 11px; font-weight: 500; letter-spacing: .2em; text-transform: uppercase;
      color: var(--gold-light); margin-bottom: 10px;
    }
    .banner-title { font-family: 'Playfair Display', serif; font-size: 28px; color: white; line-height: 1.25; margin-bottom: 10px; }
    .banner-sub { font-size: 14px; color: rgba(255,255,255,.7); }
    .btn-banner {
      background: white; color: var(--wine); border: none; cursor: pointer;
      padding: 13px 26px; border-radius: 6px;
      font-family: 'DM Sans', sans-serif; font-size: 14px; font-weight: 600;
      white-space: nowrap; transition: opacity .2s;
    }
    .btn-banner:hover { opacity: .9; }

    /* CUISINE SECTION */
    .cuisine-section { padding-bottom: 72px; }
    .section-header { display: flex; align-items: center; gap: 20px; margin-bottom: 36px; }
    .divider-line { flex: 1; height: 1px; background: var(--border); }
    .section-title { font-family: 'Playfair Display', serif; font-size: 22px; white-space: nowrap; margin: 0; }
    .cuisine-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(130px, 1fr)); gap: 14px; }
    .cuisine-card {
      display: flex; flex-direction: column; align-items: center; gap: 10px;
      padding: 24px 12px; border: 1px solid var(--border); border-radius: var(--radius);
      background: var(--card-bg); cursor: pointer; transition: all .22s;
      font-size: 13px; color: var(--ink); font-weight: 400;
    }
    .cuisine-card:hover {
      border-color: var(--wine); box-shadow: var(--shadow-sm); transform: translateY(-3px);
    }
    .cuisine-emoji { font-size: 30px; }
    .cuisine-name { font-size: 13px; color: var(--warm-gray); }

    @media (max-width: 900px) {
      .hero-inner { grid-template-columns: 1fr; gap: 40px; }
      .find-card { position: static; }
      .banner { grid-template-columns: 1fr; }
    }
  `]
})
export class HomeComponent {
  private readonly router = inject(Router);

  today = new Date().toISOString().split('T')[0];
  city = '';
  date = this.today;
  partySize = 2;
  selectedTime = '19:00';
  partySizes = [1, 2, 3, 4, 5, 6];

  timeSlots = [
    { time: '17:30', available: true },  { time: '18:00', available: true },
    { time: '18:30', available: true },  { time: '19:00', available: true },
    { time: '19:30', available: true },  { time: '20:00', available: true },
    { time: '20:30', available: true },  { time: '21:00', available: true },
    { time: '21:30', available: true },
  ];

  cuisines = [
    { name: 'Italian',  emoji: '🍝' }, { name: 'Japanese', emoji: '🍣' },
    { name: 'American', emoji: '🍔' }, { name: 'French',   emoji: '🥐' },
    { name: 'Mexican',  emoji: '🌮' }, { name: 'Seafood',  emoji: '🦞' },
    { name: 'Indian',   emoji: '🍛' }, { name: 'Thai',     emoji: '🍜' },
  ];

  setTime(t: string): void { this.selectedTime = t; }

  search(): void {
    this.router.navigate(['/restaurants'], {
      queryParams: {
        ...(this.city ? { city: this.city } : {}),
        ...(this.date ? { date: this.date } : {}),
        partySize: this.partySize,
        time: this.selectedTime
      }
    });
  }

  browse(): void { this.router.navigate(['/restaurants']); }

  browseCuisine(name: string): void {
    this.router.navigate(['/restaurants'], { queryParams: { cuisine: name } });
  }
}
