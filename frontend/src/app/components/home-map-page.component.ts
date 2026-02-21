import { Component } from '@angular/core';
import { TopBarComponent } from './top-bar.component';
import { RoutePlannerPanelComponent } from './route-planner-panel.component';
import { RouteResultsComponent } from './route-results.component';
import { MapViewComponent } from './map-view.component';

@Component({
  selector: 'app-home-map-page',
  standalone: true,
  imports: [TopBarComponent, RoutePlannerPanelComponent, RouteResultsComponent, MapViewComponent],
  template: `
    <div class="layout">
      <app-top-bar />
      <section class="content">
        <aside class="panel">
          <app-route-planner-panel />
          <app-route-results />
        </aside>
        <main class="map">
          <app-map-view />
        </main>
      </section>
    </div>
  `,
  styleUrls: ['./home-map-page.component.scss']
})
export class HomeMapPageComponent {}
