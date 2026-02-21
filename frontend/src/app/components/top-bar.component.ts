import { Component } from '@angular/core';

@Component({
  selector: 'app-top-bar',
  standalone: true,
  template: `
    <header class="top-bar">
      <div class="brand">
        <span class="dot"></span>
        <div>
          <div class="title">RouteBD</div>
          <div class="subtitle">Smart routes for Dhaka and beyond</div>
        </div>
      </div>
      <div class="tag">Powered by HERE + OpenStreetMap</div>
    </header>
  `,
  styleUrls: ['./top-bar.component.scss']
})
export class TopBarComponent {}
