import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatMenuModule } from '@angular/material/menu';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, MatMenuModule],
  template: `
    <nav class="navbar">
      <a routerLink="/" class="logo">
        <img src="assets/logo.png" alt="TableVine" class="logo-img">
      </a>
      <ul class="nav-links">
        <li><a routerLink="/restaurants" routerLinkActive="active">Discover</a></li>
        @if (auth.isLoggedIn()) {
          <li><a routerLink="/reservations" routerLinkActive="active">My Reservations</a></li>
          @if (auth.isOwner()) {
            <li><a routerLink="/owner" routerLinkActive="active">Dashboard</a></li>
          }
          @if (auth.isAdmin()) {
            <li><a routerLink="/admin" routerLinkActive="active">Admin</a></li>
          }
        }
      </ul>
      <div class="nav-end">
        @if (auth.isLoggedIn()) {
          <button class="user-btn" [matMenuTriggerFor]="userMenu">
            <span class="user-avatar">{{ initial() }}</span>
          </button>
          <mat-menu #userMenu="matMenu">
            <span mat-menu-item disabled>{{ auth.user()?.email }}</span>
            <button mat-menu-item (click)="auth.logout()">Sign out</button>
          </mat-menu>
        } @else {
          <a routerLink="/auth/login" class="nav-signin">Sign In</a>
          <a routerLink="/auth/register" class="btn-cta">Get Started</a>
        }
      </div>
    </nav>
  `,
  styles: [`
    .navbar {
      position: sticky; top: 0; z-index: 100;
      background: rgba(250,247,242,.95); backdrop-filter: blur(12px);
      border-bottom: 1px solid var(--border);
      padding: 0 40px; display: flex; align-items: center;
      justify-content: space-between; height: 68px;
    }
    .logo { text-decoration: none; display: flex; align-items: center; }
    .logo:hover { text-decoration: none; opacity: .85; transition: opacity .2s; }
    .logo-img { height: 44px; width: auto; display: block; }
    .nav-links { display: flex; gap: 32px; list-style: none; margin: 0; padding: 0; }
    .nav-links a { text-decoration: none; color: var(--warm-gray); font-size: 14px; font-weight: 400; transition: color .2s; }
    .nav-links a:hover, .nav-links a.active { color: var(--ink); text-decoration: none; }
    .nav-end { display: flex; align-items: center; gap: 12px; }
    .nav-signin { font-size: 14px; color: var(--warm-gray); text-decoration: none; transition: color .2s; }
    .nav-signin:hover { color: var(--ink); text-decoration: none; }
    .btn-cta {
      background: var(--wine); color: white !important; border: none; cursor: pointer;
      text-decoration: none !important; padding: 10px 22px; border-radius: 4px;
      font-family: 'DM Sans', sans-serif; font-size: 13px; font-weight: 500;
      letter-spacing: .04em; transition: background .2s; display: inline-block;
    }
    .btn-cta:hover { background: var(--wine-light); text-decoration: none; }
    .user-btn {
      background: none; border: 1px solid var(--border); cursor: pointer;
      border-radius: 50%; padding: 0; width: 36px; height: 36px;
      display: flex; align-items: center; justify-content: center;
    }
    .user-avatar { font-size: 14px; font-weight: 500; color: var(--wine); }
  `]
})
export class NavbarComponent {
  readonly auth = inject(AuthService);
  initial(): string {
    const u = this.auth.user();
    return (u?.firstName?.[0] ?? u?.email?.[0] ?? '?').toUpperCase();
  }
}
