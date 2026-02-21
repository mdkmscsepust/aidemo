import { Injectable } from '@angular/core';
import { BehaviorSubject, catchError, finalize, map, of, switchMap, tap } from 'rxjs';
import { RouteApiService } from './route-api.service';
import { RouteRequest, RouteState } from '../models/route.models';

const initialState: RouteState = {
  loading: false,
  routes: [],
  incidents: []
};

@Injectable({ providedIn: 'root' })
export class RouteStoreService {
  private readonly stateSubject = new BehaviorSubject<RouteState>(initialState);
  readonly state$ = this.stateSubject.asObservable();

  constructor(private readonly api: RouteApiService) {}

  planRoute(request: RouteRequest) {
    this.patchState({
      loading: true,
      error: undefined,
      from: request.from,
      to: request.to
    });

    return this.api.getRoutes(request).pipe(
      tap(response => {
        const selected = response.routes.find(route => route.rank === 1) ?? response.routes[0];
        this.patchState({
          routes: response.routes,
          selectedRouteId: selected?.id,
          incidents: []
        });
      }),
      switchMap(response => {
        const selected = response.routes.find(route => route.rank === 1) ?? response.routes[0];
        if (!selected) {
          return of({ incidents: [] });
        }
        return this.api.getIncidents(selected.bbox);
      }),
      tap(traffic => {
        this.patchState({ incidents: traffic.incidents });
      }),
      catchError(error => {
        const message = error?.error?.detail || error?.message || 'Failed to fetch routes.';
        this.patchState({ error: message, routes: [], incidents: [] });
        return of(null);
      }),
      finalize(() => {
        this.patchState({ loading: false });
      })
    ).subscribe();
  }

  selectRoute(routeId: string) {
    this.patchState({ selectedRouteId: routeId });
  }

  private patchState(patch: Partial<RouteState>) {
    const current = this.stateSubject.value;
    this.stateSubject.next({ ...current, ...patch });
  }
}
