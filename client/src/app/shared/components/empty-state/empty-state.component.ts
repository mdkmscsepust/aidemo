import { Component, Input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [MatIconModule],
  template: `
    <div class="empty-state">
      <mat-icon>{{ icon }}</mat-icon>
      <h3>{{ title }}</h3>
      @if (subtitle) { <p>{{ subtitle }}</p> }
    </div>
  `,
  styles: [`
    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 64px 24px;
      color: #9ca3af;
    }
    mat-icon { font-size: 64px; width: 64px; height: 64px; margin-bottom: 16px; }
    h3 { margin: 0 0 8px; font-size: 18px; color: #374151; }
    p { margin: 0; font-size: 14px; }
  `]
})
export class EmptyStateComponent {
  @Input() icon = 'inbox';
  @Input() title = 'Nothing here yet';
  @Input() subtitle = '';
}
