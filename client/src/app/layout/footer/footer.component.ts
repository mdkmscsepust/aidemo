import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [RouterLink],
  template: `
    <footer class="footer">
      <div class="footer-inner">
        <span class="footer-logo">TableVine</span>
        <span class="footer-copy">© {{ year }} TableVine, Inc. · All rights reserved</span>
        <div class="footer-links">
          <a routerLink="/restaurants">Discover</a>
          <a routerLink="/auth/register">Sign Up</a>
        </div>
      </div>
    </footer>
  `,
  styles: [`
    .footer {
      border-top: 1px solid var(--border);
      background: var(--cream);
    }
    .footer-inner {
      max-width: 1280px; margin: 0 auto;
      padding: 28px 40px;
      display: flex; align-items: center; justify-content: space-between;
      flex-wrap: wrap; gap: 12px;
    }
    .footer-logo {
      font-family: 'Playfair Display', serif;
      font-size: 18px; color: var(--wine);
    }
    .footer-copy { font-size: 13px; color: var(--warm-gray); }
    .footer-links { display: flex; gap: 20px; }
    .footer-links a { font-size: 13px; color: var(--warm-gray); text-decoration: none; transition: color .2s; }
    .footer-links a:hover { color: var(--ink); text-decoration: none; }
  `]
})
export class FooterComponent {
  year = new Date().getFullYear();
}
