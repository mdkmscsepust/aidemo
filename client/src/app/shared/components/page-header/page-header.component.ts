import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-page-header',
  standalone: true,
  template: `
    <div class="page-header">
      <h1>{{ title }}</h1>
      @if (subtitle) { <p class="subtitle">{{ subtitle }}</p> }
    </div>
  `,
  styles: [`
    .page-header { margin-bottom: 32px; }
    h1 { margin: 0 0 8px; font-size: 28px; font-weight: 600; color: #111827; }
    .subtitle { margin: 0; font-size: 16px; color: #6b7280; }
  `]
})
export class PageHeaderComponent {
  @Input() title = '';
  @Input() subtitle = '';
}
