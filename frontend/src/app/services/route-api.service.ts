import { Inject, Injectable, InjectionToken } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RouteRequest, RouteResponse, TrafficResponse, Bbox } from '../models/route.models';

export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');

@Injectable({ providedIn: 'root' })
export class RouteApiService {
  constructor(
    private readonly http: HttpClient,
    @Inject(API_BASE_URL) private readonly baseUrl: string
  ) {}

  getRoutes(request: RouteRequest): Observable<RouteResponse> {
    return this.http.post<RouteResponse>(`${this.baseUrl}/api/routes`, request);
  }

  getIncidents(bbox: Bbox): Observable<TrafficResponse> {
    let params = new HttpParams()
      .set('top', bbox.top)
      .set('left', bbox.left)
      .set('bottom', bbox.bottom)
      .set('right', bbox.right);

    return this.http.get<TrafficResponse>(`${this.baseUrl}/api/traffic/incidents`, { params });
  }
}
