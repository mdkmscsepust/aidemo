import { Component, Input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-star-rating',
  standalone: true,
  imports: [MatIconModule],
  template: `
    <span class="stars">
      @for (star of stars; track $index) {
        <mat-icon [style.color]="star <= rating ? '#f59e0b' : '#d1d5db'" style="font-size:18px;width:18px;height:18px;">
          {{ star <= rating ? 'star' : (star - 0.5 <= rating ? 'star_half' : 'star_border') }}
        </mat-icon>
      }
      @if (showCount && count != null) {
        <span class="count">({{ count }})</span>
      }
    </span>
  `,
  styles: [`
    .stars { display: inline-flex; align-items: center; gap: 1px; }
    .count { font-size: 12px; color: #6b7280; margin-left: 4px; }
  `]
})
export class StarRatingComponent {
  @Input() rating = 0;
  @Input() count: number | null = null;
  @Input() showCount = false;
  stars = [1, 2, 3, 4, 5];
}
