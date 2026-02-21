import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild } from '@angular/core';
import { Subscription } from 'rxjs';
import { Map, GeoJSONSource, LngLatBoundsLike } from 'maplibre-gl';
import type { Feature, FeatureCollection, LineString, Point } from 'geojson';
import { RouteStoreService } from '../services/route-store.service';
import { decodeFlexiblePolyline } from '../services/flexible-polyline';
import { RouteState } from '../models/route.models';

@Component({
  selector: 'app-map-view',
  standalone: true,
  template: '<div #mapContainer class="map"></div>',
  styleUrls: ['./map-view.component.scss']
})
export class MapViewComponent implements AfterViewInit, OnDestroy {
  @ViewChild('mapContainer', { static: true }) mapContainer!: ElementRef<HTMLDivElement>;

  private map?: Map;
  private stateSub?: Subscription;
  private mapReady = false;

  constructor(private readonly store: RouteStoreService) {}

  ngAfterViewInit(): void {
    this.map = new Map({
      container: this.mapContainer.nativeElement,
      style: {
        version: 8,
        sources: {
          osm: {
            type: 'raster',
            tiles: ['https://tile.openstreetmap.org/{z}/{x}/{y}.png'],
            tileSize: 256,
            attribution: '(c) OpenStreetMap contributors'
          }
        },
        layers: [
          {
            id: 'osm',
            type: 'raster',
            source: 'osm'
          }
        ]
      },
      center: [90.41, 23.78],
      zoom: 12
    });

    this.map.on('load', () => {
      if (!this.map) {
        return;
      }
      this.mapReady = true;
      this.map.addSource('routes', {
        type: 'geojson',
        data: { type: 'FeatureCollection', features: [] }
      });
      this.map.addLayer({
        id: 'routes-alt',
        type: 'line',
        source: 'routes',
        paint: {
          'line-color': '#94a3b8',
          'line-width': 3,
          'line-opacity': 0.6
        },
        filter: ['==', ['get', 'selected'], false]
      });
      this.map.addLayer({
        id: 'routes-selected',
        type: 'line',
        source: 'routes',
        paint: {
          'line-color': '#ef4444',
          'line-width': 5,
          'line-opacity': 0.9
        },
        filter: ['==', ['get', 'selected'], true]
      });

      this.map.addSource('incidents', {
        type: 'geojson',
        data: { type: 'FeatureCollection', features: [] }
      });
      this.map.addLayer({
        id: 'incidents-layer',
        type: 'circle',
        source: 'incidents',
        paint: {
          'circle-radius': 6,
          'circle-color': '#f59e0b',
          'circle-stroke-color': '#78350f',
          'circle-stroke-width': 1
        }
      });
    });

    this.stateSub = this.store.state$.subscribe(state => this.updateMap(state));
  }

  ngOnDestroy(): void {
    this.stateSub?.unsubscribe();
    this.map?.remove();
  }

  private updateMap(state: RouteState): void {
    if (!this.mapReady || !this.map) {
      return;
    }

    const routeFeatures: Feature<LineString>[] = state.routes.map(route => ({
      type: 'Feature',
      geometry: {
        type: 'LineString',
        coordinates: decodeFlexiblePolyline(route.polyline)
      },
      properties: {
        id: route.id,
        selected: route.id === state.selectedRouteId
      }
    }));

    const routeSource = this.map.getSource('routes') as GeoJSONSource | undefined;
    const routeCollection: FeatureCollection<LineString> = {
      type: 'FeatureCollection',
      features: routeFeatures
    };
    routeSource?.setData(routeCollection);

    const incidentFeatures: Feature<Point>[] = state.incidents.map(incident => ({
      type: 'Feature',
      geometry: {
        type: 'Point',
        coordinates: [incident.lng, incident.lat]
      },
      properties: {
        id: incident.id
      }
    }));

    const incidentSource = this.map.getSource('incidents') as GeoJSONSource | undefined;
    const incidentCollection: FeatureCollection<Point> = {
      type: 'FeatureCollection',
      features: incidentFeatures
    };
    incidentSource?.setData(incidentCollection);

    const selected = state.routes.find(route => route.id === state.selectedRouteId);
    if (selected) {
      const bounds: LngLatBoundsLike = [
        [selected.bbox.left, selected.bbox.bottom],
        [selected.bbox.right, selected.bbox.top]
      ];
      this.map.fitBounds(bounds, { padding: 60, duration: 800 });
    }
  }
}
